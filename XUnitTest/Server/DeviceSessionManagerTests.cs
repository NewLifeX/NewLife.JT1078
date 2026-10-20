using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;
using NewLife.Messaging;
using Xunit;

namespace XUnitTest.Server;

/// <summary>设备命令总线测试</summary>
public class DeviceSessionManagerTests
{
    [Fact(DisplayName = "命令广播：十六进制负载编码")]
    public async Task Publish_BroadcastUsesHexPayload()
    {
        using var mgr = new DeviceSessionManager(null) { Bus = new EventBus<String>() };
        mgr.Init();

        var tcs = new TaskCompletionSource<String>(TaskCreationOptions.RunContinuationsAsynchronously);
        EventBusExtensions.Subscribe<String>(mgr.Bus, (m, ctx) => tcs.TrySetResult(m));

        var payload = new ArrayPacket(new Byte[] { 0x12, 0x34, 0xAB });
        Assert.True(mgr.Publish("138001380000", 1, AVTypes.H264, DataTypes.Transparent, payload));

        var message = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(3));
        Assert.Equal("138001380000_1#H264|4|1234AB", message);
    }

    [Fact(DisplayName = "设备会话注册与查找")]
    public void Register_Find()
    {
        using var mgr = new DeviceSessionManager(null);

        mgr.Register("138001380000_1", new Object());
        Assert.True(mgr.Contains("138001380000_1"));
        Assert.NotNull(mgr.GetSession("138001380000_1"));
        Assert.Single(mgr.GetSessions());

        mgr.Unregister("138001380000_1");
        Assert.False(mgr.Contains("138001380000_1"));
    }

    [Fact(DisplayName = "注销会话：仅移除同一会话")]
    public void Unregister_OnlySameSession()
    {
        using var mgr = new DeviceSessionManager(null);

        var oldSession = new Object();
        var newSession = new Object();

        mgr.Register("138001380000_1", newSession);
        Assert.False(mgr.Unregister("138001380000_1", oldSession));
        Assert.True(mgr.Contains("138001380000_1"));

        Assert.True(mgr.Unregister("138001380000_1", newSession));
        Assert.False(mgr.Contains("138001380000_1"));
    }

    [Fact(DisplayName = "清理超时 UDP 设备会话")]
    public void RemoveExpired_UdpSession()
    {
        using var mgr = new DeviceSessionManager(null);

        mgr.Register("old", new JT1078UdpSession { LastActive = DateTime.Now.AddMinutes(-5) });
        mgr.Register("fresh", new JT1078UdpSession { LastActive = DateTime.Now });

        Assert.Equal(1, mgr.RemoveExpired(TimeSpan.FromMinutes(1)));
        Assert.False(mgr.Contains("old"));
        Assert.True(mgr.Contains("fresh"));
    }
}
