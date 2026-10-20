using System.Buffers;
using System.Net;
using System.Net.Sockets;
using NewLife.Caching;
using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;
using Xunit;

namespace XUnitTest.Server;

/// <summary>JT1078 服务端协议模式集成测试</summary>
public class ServerProtocolTests
{
    private static Int32 GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    private static JTMessage CreateMessage(Int32 sequence, Byte[] payload, PacketFlags flag = PacketFlags.Alone, Int32 channel = 1) => new()
    {
        Version = 129,
        Type = AVTypes.H264,
        Completed = true,
        Sequence = (UInt16)sequence,
        SIM = "138001380000",
        Channel = (Byte)channel,
        DataType = DataTypes.VideoIFrame,
        PacketFlag = flag,
        Timestamp = (UInt64)sequence * 40,
        Payload = new ArrayPacket(payload),
    };

    private static async Task<JT1078Session> WaitSession(JT1078Server server, Int32 timeoutMs = 5000)
    {
        var end = DateTime.Now.AddMilliseconds(timeoutMs);
        while (DateTime.Now < end)
        {
            if (server.Sessions.Values.FirstOrDefault() is JT1078Session session) return session;
            await Task.Delay(20);
        }

        throw new TimeoutException("No session was created.");
    }

    private static async Task<Byte[]> ReadFrameAsync(NetworkStream stream, JTCodec codec, Int32 timeoutMs = 5000)
    {
        using var cts = new CancellationTokenSource(timeoutMs);
        var cache = new MemoryStream();
        var buffer = new Byte[128];

        while (true)
        {
            if (cache.Length > 0)
            {
                if (!cache.TryGetBuffer(out var seg)) break;

                var rs = codec.TryParse(new ReadOnlySequence<Byte>(seg.AsMemory(0, (Int32)cache.Length)));
                if (rs != null)
                {
                    var total = rs.Value.HeaderSize + (Int32)rs.Value.BodyLength;
                    if (cache.Length >= total) return cache.ToArray()[..total];
                }
            }

            var count = await stream.ReadAsync(buffer, cts.Token);
            if (count == 0) break;

            cache.Write(buffer, 0, count);
        }

        throw new TimeoutException("No complete frame was received.");
    }

    [Fact(DisplayName = "分包合并：无 SIM 直接按独立包处理")]
    public void Merge_NoSim()
    {
        using var server = new JT1078Server { Cache = new MemoryCache() };

        var msg = new JTMessage
        {
            PacketFlag = PacketFlags.First,
            Payload = new ArrayPacket(new Byte[] { 1, 2, 3 }),
        };

        var session = new JT1078Session();
        Assert.Same(msg, server.Merge(session, msg));
    }

    [Fact(DisplayName = "分包合并：首包+中间包+末包")]
    public void Merge_Fragments()
    {
        using var server = new JT1078Server { Cache = new MemoryCache() };
        var session = new JT1078Session();

        JTMessage Make(PacketFlags flag, Byte[] payload) => new()
        {
            Version = 129,
            Type = AVTypes.H264,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = flag,
            Payload = new ArrayPacket(payload),
        };

        Assert.Null(server.Merge(session, Make(PacketFlags.First, [1, 2])));
        Assert.Null(server.Merge(session, Make(PacketFlags.Middle, [3, 4])));

        var merged = server.Merge(session, Make(PacketFlags.Last, [5, 6]));
        Assert.NotNull(merged);
        Assert.Equal(new Byte[] { 1, 2, 3, 4, 5, 6 }, merged!.Payload!.ToArray());
    }

    private static Int32 GetFreeUdpPort()
    {
        using var client = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)client.Client.LocalEndPoint!).Port;
    }

    [Fact(DisplayName = "UDP 服务端：数据报解析并注册设备")]
    public async Task UdpServer_ReceiveAndRegister()
    {
        var port = GetFreeUdpPort();
        using var manager = new DeviceSessionManager(null);
        using var server = new JT1078UdpServer
        {
            Setting = new JT1078Setting { UdpPort = port },
            DeviceManager = manager,
        };
        server.Start();

        try
        {
            using var client = new UdpClient();
            var frame = CreateMessage(1, [1, 2, 3], channel: 2).ToPacket().GetSpan().ToArray();
            await client.SendAsync(frame, new IPEndPoint(IPAddress.Loopback, port));

            var end = DateTime.Now.AddSeconds(5);
            while (DateTime.Now < end && !manager.Contains("138001380000_2")) await Task.Delay(20);

            Assert.True(manager.Contains("138001380000_2"));
        }
        finally
        {
            server.Stop();
        }
    }

    [Fact(DisplayName = "分包合并：缺少首包时丢弃残缺帧")]
    public void Merge_OrphanFragment()
    {
        using var server = new JT1078Server { Cache = new MemoryCache() };
        var session = new JT1078Session();

        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Last,
            Payload = new ArrayPacket(new Byte[] { 1, 2, 3 }),
        };

        Assert.Null(server.Merge(session, msg));
    }

    [Fact(DisplayName = "服务端协议模式：粘包拆分为多条消息")]
    public async Task Server_StickyFrames()
    {
        var port = GetFreePort();
        using var server = new JT1078Server { Port = port, Cache = new MemoryCache() };
        server.Setting.Port = port;
        server.Start();

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            var session = await WaitSession(server);
            var received = new List<(Int32 Sequence, Byte[] Data)>();
            using var signal = new SemaphoreSlim(0);
            session.OnReceiveMessage += (s, m) =>
            {
                lock (received) received.Add((m.Sequence, m.Payload?.ToArray() ?? []));
                signal.Release();
            };

            var f1 = CreateMessage(1, [1, 2, 3]).ToPacket().GetSpan().ToArray();
            var f2 = CreateMessage(2, [4, 5, 6, 7]).ToPacket().GetSpan().ToArray();

            var stream = client.GetStream();
            await stream.WriteAsync(f1.Concat(f2).ToArray());

            Assert.True(await signal.WaitAsync(5000));
            Assert.True(await signal.WaitAsync(5000));

            lock (received)
            {
                Assert.Equal(2, received.Count);
                Assert.Equal(1, received[0].Sequence);
                Assert.Equal(3, received[0].Data.Length);
                Assert.Equal(2, received[1].Sequence);
                Assert.Equal(4, received[1].Data.Length);
            }
        }
        finally
        {
            server.Stop("test");
        }
    }

    [Fact(DisplayName = "服务端协议模式：拆包重新组装")]
    public async Task Server_FragmentedFrame()
    {
        var port = GetFreePort();
        using var server = new JT1078Server { Port = port, Cache = new MemoryCache() };
        server.Setting.Port = port;
        server.Start();

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            var session = await WaitSession(server);
            using var signal = new SemaphoreSlim(0);
            (Int32 Sequence, Byte[] Data)? received = null;
            session.OnReceiveMessage += (s, m) =>
            {
                received = (m.Sequence, m.Payload?.ToArray() ?? []);
                signal.Release();
            };

            var frame = CreateMessage(9, [9, 8, 7, 6, 5, 4, 3, 2, 1]).ToPacket().GetSpan().ToArray();
            var stream = client.GetStream();

            await stream.WriteAsync(frame.AsMemory(0, 12));
            await Task.Delay(100);
            await stream.WriteAsync(frame.AsMemory(12));

            Assert.True(await signal.WaitAsync(5000));
            Assert.NotNull(received);
            Assert.Equal(9, received!.Value.Sequence);
            Assert.Equal(9, received.Value.Data.Length);
        }
        finally
        {
            server.Stop("test");
        }
    }

    [Fact(DisplayName = "服务端协议模式：分包合并为完整帧")]
    public async Task Server_MergeFragments()
    {
        var port = GetFreePort();
        using var server = new JT1078Server { Port = port, Cache = new MemoryCache() };
        server.Setting.Port = port;
        server.Start();

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            var session = await WaitSession(server);
            using var signal = new SemaphoreSlim(0);
            var received = new List<Byte[]>();
            session.OnReceiveMessage += (s, m) =>
            {
                lock (received) received.Add(m.Payload?.ToArray() ?? []);
                signal.Release();
            };

            var stream = client.GetStream();
            var first = CreateMessage(1, [1, 2], PacketFlags.First).ToPacket().GetSpan().ToArray();
            var middle = CreateMessage(2, [3, 4], PacketFlags.Middle).ToPacket().GetSpan().ToArray();
            var last = CreateMessage(3, [5, 6], PacketFlags.Last).ToPacket().GetSpan().ToArray();

            await stream.WriteAsync(first.Concat(middle).ToArray());
            await Task.Delay(100);
            await stream.WriteAsync(last);

            Assert.True(await signal.WaitAsync(5000));

            lock (received)
            {
                var data = Assert.Single(received);
                Assert.Equal(new Byte[] { 1, 2, 3, 4, 5, 6 }, data);
            }
        }
        finally
        {
            server.Stop("test");
        }
    }

    [Fact(DisplayName = "服务端协议模式：流式消息体物化")]
    public async Task Server_StreamingBody()
    {
        var port = GetFreePort();
        using var server = new JT1078Server { Port = port, Cache = new MemoryCache() };
        server.Setting.Port = port;
        server.Start();

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            var session = await WaitSession(server);
            using var signal = new SemaphoreSlim(0);
            var received = new List<Byte[]>();
            session.OnReceiveMessage += (s, m) =>
            {
                lock (received) received.Add(m.Payload?.ToArray() ?? []);
                signal.Release();
            };

            // 先发头部，令消息体处于流式读取状态，再补发消息体
            var payload = new Byte[4096];
            for (var i = 0; i < payload.Length; i++) payload[i] = (Byte)(i & 0xFF);
            var frame = CreateMessage(5, payload).ToPacket().GetSpan().ToArray();

            var stream = client.GetStream();
            await stream.WriteAsync(frame.AsMemory(0, 30));
            await Task.Delay(100);
            await stream.WriteAsync(frame.AsMemory(30));

            Assert.True(await signal.WaitAsync(5000));

            lock (received)
            {
                var data = Assert.Single(received);
                Assert.Equal(payload, data);
            }
        }
        finally
        {
            server.Stop("test");
        }
    }

    [Fact(DisplayName = "服务端协议模式：下行信令经协议构建发送")]
    public async Task Server_SendJT()
    {
        var port = GetFreePort();
        using var server = new JT1078Server { Port = port, Cache = new MemoryCache() };
        server.Setting.Port = port;
        server.Start();

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(IPAddress.Loopback, port);

            var session = await WaitSession(server);
            using var signal = new SemaphoreSlim(0);
            session.OnReceiveMessage += (s, m) => signal.Release();

            var stream = client.GetStream();

            // 设备先上行一帧，服务端记录 SIM
            await stream.WriteAsync(CreateMessage(1, [1, 2]).ToPacket().GetSpan().ToArray());
            Assert.True(await signal.WaitAsync(5000));

            var payload = new Byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
            var sent = session.SendJT(AVTypes.H264, DataTypes.VideoIFrame, new ArrayPacket(payload));
            Assert.True(sent > 0);

            var frame = await ReadFrameAsync(stream, new JTCodec());
            var msg = new JTMessage();
            Assert.True(msg.Read(new ArrayPacket(frame)));

            Assert.Equal("138001380000", msg.SIM);
            Assert.Equal(DataTypes.VideoIFrame, msg.DataType);
            Assert.Equal(1, msg.Channel);
            Assert.Equal(payload, msg.Payload.GetSpan().ToArray());
        }
        finally
        {
            server.Stop("test");
        }
    }
}
