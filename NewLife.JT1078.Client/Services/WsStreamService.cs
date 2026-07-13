using System.Net.WebSockets;

namespace NewLife.JT1078.Client.Services;

/// <summary>WebSocket 流接收服务。接收服务器推送的音视频流数据</summary>
public class WsStreamService : IDisposable
{
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;

    /// <summary>是否已连接</summary>
    public Boolean IsConnected => _webSocket?.State == WebSocketState.Open;

    /// <summary>收到数据时触发。参数为原始字节数据</summary>
    public event Action<Byte[]>? OnDataReceived;

    /// <summary>连接状态变化时触发</summary>
    public event Action<Boolean>? OnConnectionChanged;

    /// <summary>错误发生时触发</summary>
    public event Action<Exception>? OnError;

    /// <summary>连接到 WebSocket 直播端点</summary>
    public async Task ConnectAsync(String url, CancellationToken cancellationToken = default)
    {
        await DisconnectAsync();

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _webSocket = new ClientWebSocket();

        try
        {
            await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
            OnConnectionChanged?.Invoke(true);

            // 启动接收循环
            _receiveTask = ReceiveLoopAsync(_cts.Token);
        }
        catch (Exception ex)
        {
            OnError?.Invoke(ex);
            throw;
        }
    }

    /// <summary>断开连接</summary>
    public async Task DisconnectAsync()
    {
        _cts?.Cancel();

        if (_webSocket != null)
        {
            try
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "客户端关闭", CancellationToken.None);
                }
            }
            catch { }
            finally
            {
                _webSocket.Dispose();
                _webSocket = null;
            }
        }

        if (_receiveTask != null)
        {
            try { await _receiveTask; } catch { }
            _receiveTask = null;
        }

        _cts?.Dispose();
        _cts = null;

        OnConnectionChanged?.Invoke(false);
    }

    /// <summary>接收循环</summary>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        var buffer = new Byte[8192];

        try
        {
            while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<Byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure, "服务器关闭", CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Binary && result.Count > 0)
                {
                    var data = new Byte[result.Count];
                    Array.Copy(buffer, data, result.Count);
                    OnDataReceived?.Invoke(data);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (WebSocketException ex)
        {
            OnError?.Invoke(ex);
        }
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
        _webSocket?.Dispose();
        _cts?.Dispose();
    }
}
