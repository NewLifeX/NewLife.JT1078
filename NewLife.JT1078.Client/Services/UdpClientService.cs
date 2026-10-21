using System.Net.Sockets;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.Services;

/// <summary>UDP 客户端服务。直连 JT1078 UDP 端口收发数据</summary>
public class UdpClientService : IDisposable
{
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;
    private String _remoteHost = "";
    private Int32 _remotePort;

    /// <summary>是否已连接</summary>
    public Boolean IsConnected => _udpClient != null;

    /// <summary>收到 JTMessage 时触发</summary>
    public event Action<JTMessage>? OnMessageReceived;

    /// <summary>收到原始数据时触发</summary>
    public event Action<Byte[]>? OnRawDataReceived;

    /// <summary>连接状态变化</summary>
    public event Action<Boolean>? OnConnectionChanged;

    /// <summary>错误发生</summary>
    public event Action<Exception>? OnError;

    /// <summary>连接到 JT1078 UDP 端口</summary>
    public void Connect(String host, Int32 port)
    {
        Disconnect();

        _remoteHost = host;
        _remotePort = port;
        _cts = new CancellationTokenSource();

        _udpClient = new UdpClient();
        _udpClient.Connect(host, port);

        OnConnectionChanged?.Invoke(true);

        _receiveTask = ReceiveLoopAsync(_cts.Token);
    }

    /// <summary>断开连接</summary>
    public void Disconnect()
    {
        _cts?.Cancel();

        if (_receiveTask != null)
        {
            try { _receiveTask.Wait(1000); } catch { }
            _receiveTask = null;
        }

        _udpClient?.Close();
        _udpClient?.Dispose();
        _udpClient = null;

        _cts?.Dispose();
        _cts = null;

        OnConnectionChanged?.Invoke(false);
    }

    /// <summary>发送 JTMessage</summary>
    public async Task SendAsync(JTMessage message)
    {
        if (_udpClient == null)
            throw new InvalidOperationException(Localization.Translate("Not connected to the server."));

        var packet = message.ToPacket();
        var data = packet.GetSpan().ToArray();
        await _udpClient.SendAsync(data);
    }

    /// <summary>发送原始数据</summary>
    public async Task SendRawAsync(Byte[] data)
    {
        if (_udpClient == null)
            throw new InvalidOperationException(Localization.Translate("Not connected to the server."));

        await _udpClient.SendAsync(data);
    }

    /// <summary>接收循环</summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _udpClient != null)
            {
                var result = await _udpClient.ReceiveAsync(cancellationToken);
                OnRawDataReceived?.Invoke(result.Buffer);

                // 尝试解析为 JTMessage
                try
                {
                    var msg = new JTMessage();
                    using var ms = new MemoryStream(result.Buffer);
                    if (msg.Read(ms, null!))
                    {
                        OnMessageReceived?.Invoke(msg);
                    }
                }
                catch
                {
                    // 非 JTMessage 格式则忽略
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
        _udpClient?.Dispose();
        _cts?.Dispose();
    }
}
