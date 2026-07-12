using NewLife.JT1078.Server;
using Microsoft.Extensions.Hosting;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 后台服务。作为 ASP.NET Core IHostedService 管理 JT1078Server 生命周期</summary>
public class JT1078HostedService : IHostedService
{
    #region 属性
    private NetServer _tcpServer;
    private JT1078UdpServer _udpServer;
    private readonly ITracer _tracer;
    #endregion

    #region 构造
    /// <summary>实例化 JT1078 后台服务</summary>
    /// <param name="tracer">链路追踪</param>
    public JT1078HostedService(ITracer tracer)
    {
        _tracer = tracer;
    }
    #endregion

    #region 方法
    /// <summary>启动服务</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        var set = JT1078Setting.Current;

        // 启动 TCP 服务
        if (set.Port > 0)
        {
            var svr = new NewLife.JT1078.Server.JT1078Server
            {
                Port = set.Port,
                Tracer = _tracer,
                Log = XTrace.Log,
            };

            if (set.Debug)
            {
                svr.SessionLog = XTrace.Log;
                svr.SocketLog = XTrace.Log;
                svr.LogSend = true;
                svr.LogReceive = true;
            }

            svr.Start();
            _tcpServer = svr;
        }

        // 启动 UDP 服务
        if (set.UdpPort > 0)
        {
            _udpServer = new JT1078UdpServer
            {
                Setting = set,
            };
            _udpServer.Start();
        }

        return Task.CompletedTask;
    }

    /// <summary>停止服务</summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        lock (this)
        {
            _tcpServer?.Stop(nameof(StopAsync));
            _tcpServer = null;

            _udpServer?.Stop();
            _udpServer = null;
        }

        return Task.CompletedTask;
    }
    #endregion
}
