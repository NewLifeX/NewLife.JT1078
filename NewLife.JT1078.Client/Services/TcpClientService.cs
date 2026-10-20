using System.Net.Sockets;
using NewLife.Data;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.Services;

/// <summary>TCP 客户端服务。直连 JT1078 TCP 端口，使用 JTCodec 粘包解码</summary>
public class TcpClientService : IDisposable
{
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private readonly JTCodec _codec = new();
    private readonly MemoryStream _cache = new();
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

        _cache.SetLength(0);

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

                // 追加到缓存，按 JT1078 帧格式循环解包
                _cache.Write(buffer, 0, bytesRead);
                DecodeCache();
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

    /// <summary>从缓存中循环解包完整帧并触发消息事件</summary>
    private void DecodeCache()
    {
        while (_cache.Length > 0)
        {
            if (!_cache.TryGetBuffer(out var seg)) return;

            var data = seg.AsMemory(0, (Int32)_cache.Length);
            var rs = _codec.TryParse(new System.Buffers.ReadOnlySequence<Byte>(data));
            if (rs == null) return;

            var result = rs.Value;
            if (result.Invalid)
            {
                _cache.SetLength(0);
                OnError?.Invoke(new InvalidDataException("Invalid JT1078 frame."));
                return;
            }

            var total = result.HeaderSize + (Int32)result.BodyLength;
            if (_cache.Length < total) return;

            var frame = new Byte[total];
            Buffer.BlockCopy(seg.Array!, seg.Offset, frame, 0, total);

            var remain = (Int32)_cache.Length - total;
            if (remain > 0) Buffer.BlockCopy(seg.Array!, seg.Offset + total, seg.Array!, seg.Offset, remain);
            _cache.SetLength(remain);

            var msg = new JTMessage();
            if (msg.Read(new ArrayPacket(frame))) OnMessageReceived?.Invoke(msg);
        }
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _cts?.Cancel();
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _cts?.Dispose();
        _cache.Dispose();
    }
}
