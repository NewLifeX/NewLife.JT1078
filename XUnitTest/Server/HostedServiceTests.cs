using System.Net;
using System.Net.Sockets;
using NewLife.JT1078.Server;
using Xunit;

namespace XUnitTest.Server;

/// <summary>JT1078 宿主服务测试。配置为进程级全局状态，禁止并行</summary>
[Collection("GlobalState")]
public class HostedServiceTests
{
    private sealed class EmptyProvider : IServiceProvider
    {
        public Object GetService(Type serviceType) => null;
    }

    private static Int32 GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    [Fact(DisplayName = "宿主服务启动的是注入的同一服务端实例")]
    public async Task StartAsync_UsesInjectedInstances()
    {
        var set = JT1078Setting.Current;
        var oldPort = set.Port;
        var oldUdpPort = set.UdpPort;

        var port = GetFreePort();
        set.Port = port;
        set.UdpPort = 0;

        using var server = new JT1078Server();
        using var udp = new JT1078UdpServer();
        using var manager = new DeviceSessionManager(null);
        var hosted = new JT1078HostedService(server, udp, manager, new EmptyProvider());

        try
        {
            await hosted.StartAsync(default);

            Assert.True(server.Active);
            Assert.Same(manager, server.DeviceManager);
        }
        finally
        {
            await hosted.StopAsync(default);
            set.Port = oldPort;
            set.UdpPort = oldUdpPort;
        }
    }
}
