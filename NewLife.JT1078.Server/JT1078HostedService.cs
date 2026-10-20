using NewLife.JT1078.Server;
using Microsoft.Extensions.Hosting;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 后台服务。作为 ASP.NET Core IHostedService 管理 JT1078Server 生命周期</summary>
public class JT1078HostedService : IHostedService
{
    #region 属性
    private readonly JT1078Server _tcpServer;
    private readonly JT1078UdpServer _udpServer;
    private readonly DeviceSessionManager _deviceManager;
    private readonly IServiceProvider _provider;
    #endregion

    #region 构造
    /// <summary>实例化 JT1078 后台服务</summary>
    /// <param name="server">TCP 服务端（DI 单例）</param>
    /// <param name="udpServer">UDP 服务端（DI 单例）</param>
    /// <param name="deviceManager">设备命令总线（DI 单例）</param>
    /// <param name="provider">服务提供者</param>
    public JT1078HostedService(JT1078Server server, JT1078UdpServer udpServer, DeviceSessionManager deviceManager, IServiceProvider provider)
    {
        _tcpServer = server;
        _udpServer = udpServer;
        _deviceManager = deviceManager;
        _provider = provider;
    }
    #endregion

    #region 方法
    /// <summary>启动服务</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var set = JT1078Setting.Current;
        var tracer = _provider.GetService(typeof(ITracer)) as ITracer;

        // 启动 TCP 服务。使用 DI 单例，确保控制器与运行中的服务端共享设备会话
        if (set.Port > 0)
        {
            _tcpServer.Port = set.Port;
            _tcpServer.Setting = set;
            _tcpServer.Tracer = tracer;
            _tcpServer.Log = XTrace.Log;
            _tcpServer.ServiceProvider = _provider;
            _tcpServer.DeviceManager = _deviceManager;

            if (set.Debug)
            {
                _tcpServer.SessionLog = XTrace.Log;
                _tcpServer.SocketLog = XTrace.Log;
                _tcpServer.LogSend = true;
                _tcpServer.LogReceive = true;
            }

            _tcpServer.Start();
        }

        // 启动 UDP 服务
        if (set.UdpPort > 0)
        {
            _udpServer.Setting = set;
            _udpServer.ServiceProvider = _provider;
            _udpServer.DeviceManager = _deviceManager;
            _udpServer.Start();
        }

        return Task.CompletedTask;
    }

    /// <summary>停止服务</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _tcpServer.Stop(nameof(StopAsync));
        _udpServer.Stop();

        return Task.CompletedTask;
    }
    #endregion
}
