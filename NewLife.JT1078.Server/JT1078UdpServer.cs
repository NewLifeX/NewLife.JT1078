using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 UDP 服务端。接收车载终端上传的 RTP 音视频流数据（UDP 传输模式）</summary>
/// <remarks>
/// JT/T 1078 支持 UDP 传输模式，每个 UDP 数据报包含一个完整的 RTP 报文。
/// 与 TCP 模式不同，UDP 无需粘包处理。
/// </remarks>
public class JT1078UdpServer : DisposeBase, IDisposable2
{
    #region 属性
    /// <summary>JT1078 配置</summary>
    public JT1078Setting Setting { get; set; } = new();

    /// <summary>服务端 Socket</summary>
    private System.Net.Sockets.Socket _server;

    /// <summary>取消令牌</summary>
    private CancellationTokenSource _cts;

    /// <summary>设备命令总线</summary>
    public DeviceSessionManager DeviceManager { get; set; }

    /// <summary>服务提供者</summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>是否正在运行</summary>
    public Boolean Running { get; private set; }
    #endregion

    #region 构造
    /// <summary>实例化 UDP 服务端</summary>
    public JT1078UdpServer()
    {
        Name = "JT1078Udp";
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Stop();
        _server.TryDispose();
        _cts.TryDispose();
    }

    /// <summary>名称</summary>
    public String Name { get; set; }
    #endregion

    #region 启动/停止
    /// <summary>启动 UDP 服务</summary>
    public void Start()
    {
        if (Running) return;

        var port = Setting.UdpPort > 0 ? Setting.UdpPort : 1078;
        var address = System.Net.IPAddress.Any;
        var endPoint = new System.Net.IPEndPoint(address, port);

        _server = new System.Net.Sockets.Socket(address.AddressFamily, System.Net.Sockets.SocketType.Dgram, System.Net.Sockets.ProtocolType.Udp);
        _server.Bind(endPoint);

        _cts = new CancellationTokenSource();
        Running = true;

        // 初始化设备命令总线
        DeviceManager ??= new DeviceSessionManager(ServiceProvider);
        DeviceManager.Init();

        // 启动接收线程
        System.Threading.ThreadPool.QueueUserWorkItem(_ => ReceiveLoop());

        WriteLog("UDP server started on port {0}", port);
    }

    /// <summary>停止 UDP 服务</summary>
    public void Stop()
    {
        if (!Running) return;

        Running = false;
        _cts?.Cancel();

        try
        {
            _server?.Close();
        }
        catch { }

        WriteLog("UDP server stopped");
    }

    /// <summary>接收循环</summary>
    private void ReceiveLoop()
    {
        var bufferSize = Setting.BufferSize > 0 ? Setting.BufferSize : 8192;

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                var buffer = new Byte[bufferSize];
                var remoteEndPoint = new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0) as System.Net.EndPoint;

                var receivedBytes = _server.ReceiveFrom(buffer, System.Net.Sockets.SocketFlags.None, ref remoteEndPoint);

                if (receivedBytes > 0)
                {
                    var data = new Byte[receivedBytes];
                    Buffer.BlockCopy(buffer, 0, data, 0, receivedBytes);
                    ProcessDatagram(data, remoteEndPoint);
                }
            }
            catch (ObjectDisposedException) { break; }
            catch (System.Net.Sockets.SocketException) { break; }
            catch (Exception ex)
            {
                WriteLog("Receive error: {0}", ex.Message);
            }
        }
    }
    #endregion

    #region 数据处理
    /// <summary>处理收到的 UDP 数据报</summary>
    private void ProcessDatagram(Byte[] data, System.Net.EndPoint remoteEndPoint)
    {
        try
        {
            var msg = new JTMessage();
            var packet = new ArrayPacket(data);
            msg.Read(packet);

            // 查找或注册设备会话
            var key = $"{msg.SIM}_{msg.Channel}";
            var session = DeviceManager?.GetSession(key);

            if (session == null)
            {
                // UDP 设备首次上线，注册到设备命令总线
                var udpSession = new JT1078UdpSession
                {
                    DeviceKey = key,
                    RemoteEndPoint = remoteEndPoint,
                    SIM = msg.SIM,
                    Channel = msg.Channel,
                };
                DeviceManager?.Register(key, udpSession);
                WriteLog("UDP device online SIM={0} Ch={1} EP={2}", msg.SIM, msg.Channel, remoteEndPoint);
            }
            else if (session is JT1078UdpSession udpSession)
            {
                udpSession.RemoteEndPoint = remoteEndPoint;
                udpSession.LastActive = DateTime.Now;
            }
        }
        catch (Exception ex)
        {
            WriteLog("Process error: {0}", ex.Message);
        }
    }
    #endregion

    #region 日志
    private void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.InfoPrefixed(Name, format, args);
    }
    #endregion
}
