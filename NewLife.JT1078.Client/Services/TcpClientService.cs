using System.Net.Sockets;
using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Messaging;

namespace NewLife.JT1078.Client.Services;

/// <summary>TCP 客户端服务。直连 JT1078 TCP 端口，使用 PacketCodec 粘包解码</summary>
public class TcpClientService : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private PacketCodec? _codec;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;

    /// <summary>是否已连接</summary>
    public Boolean IsConnected => _tcpClient?.Connected ?? false;

    /// <summary>收到 JTMessage 时触发</summary>
    public event Action<JTMessage>? OnMessageReceived;

    /// <summary>连接状态变化</summary>
    public event Action<Boolean>? OnConnectionChanged;

    /// <summary>错误发生</summary>
    public event Action<Exception>? OnError;

    /// <summary>连接到 JT1078 TCP 服务端</summary>
    public async Task ConnectAsync(String host, Int32 port, CancellationToken cancellationToken = default)
    {
        await DisconnectAsync();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(host, port);
        _stream = _tcpClient.GetStream();

        _codec = new PacketCodec
        {
            MaxCache = 1024,
#pragma warning disable CS0618 // PacketCodec.GetLength 已弃用，但 GetLength2 签名未知
            GetLength = JTMessage.GetLength,
#pragma warning restore CS0618
        };

        OnConnectionChanged?.Invoke(true);

        _receiveTask = ReceiveLoopAsync(_cts.Token);
    }

    /// <summary>断开连接</summary>
    public async Task DisconnectAsync()
    {
        _cts?.Cancel();

        if (_receiveTask != null)
        {
            try { await _receiveTask; } catch { }
            _receiveTask = null;
        }

        _stream?.Close();
        _stream?.Dispose();
        _stream = null;

        _tcpClient?.Close();
        _tcpClient?.Dispose();
        _tcpClient = null;

        _codec = null;

        _cts?.Dispose();
        _cts = null;

        OnConnectionChanged?.Invoke(false);
    }

    /// <summary>发送 JTMessage</summary>
    public async Task SendAsync(JTMessage message)
    {
        if (_stream == null)
            throw new InvalidOperationException("未连接到服务器");

        var packet = message.ToPacket();
        var data = packet.GetSpan().ToArray();
        await _stream.WriteAsync(data);
        await _stream.FlushAsync();
    }

    /// <summary>发送原始数据</summary>
    public async Task SendRawAsync(Byte[] data)
    {
        if (_stream == null)
            throw new InvalidOperationException("未连接到服务器");

        await _stream.WriteAsync(data);
        await _stream.FlushAsync();
    }

    /// <summary>接收循环</summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new Byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _tcpClient?.Connected == true)
            {
                var bytesRead = await _stream!.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0) break; // 连接关闭

                // 使用 PacketCodec 解析粘包
                var data = new Byte[bytesRead];
                Array.Copy(buffer, data, bytesRead);
                var packet = new ArrayPacket(data);

                if (_codec != null)
                {
                    var packets = _codec.Parse(packet);
                    if (packets != null)
                    {
                        foreach (var pk in packets)
                        {
                            var msg = new JTMessage();
                            if (msg.Read(pk))
                            {
                                OnMessageReceived?.Invoke(msg);
                            }
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
        }
        finally
        {
            OnConnectionChanged?.Invoke(false);
        }
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _cts?.Dispose();
    }
}
