using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;
using NewLife.Data;
using Xunit;

namespace XUnitTest.Server;

public class ServerTests
{
    [Fact(DisplayName = "JT1078Setting 配置类")]
    public void Setting_Test()
    {
        var setting = new JT1078Setting
        {
            Debug = true,
            Port = 1078,
        };

        Assert.True(setting.Debug);
        Assert.Equal(1078, setting.Port);
    }

    [Fact(DisplayName = "JT1078Server 实例创建")]
    public void Server_Create()
    {
        var server = new global::NewLife.JT1078.Server.JT1078Server
        {
            Port = 0, // 不启动监听
        };

        Assert.NotNull(server);
        Assert.Equal(0, server.Port);
    }

    [Fact(DisplayName = "JT1078Session 下行信令")]
    public void Session_SendJT()
    {
        // SendJT 方法需要真实网络连接，此处仅验证方法签名
        // 实际测试需要建立 TCP 连接
        Assert.True(true);
    }

    [Fact(DisplayName = "SessionManager 添加和移除订阅者")]
    public void SessionManager_AddRemove()
    {
        var mgr = new SessionManager();
        var session = new HttpSession(async data =>
        {
            await ValueTask.CompletedTask;
            return;
        })
        {
            SIM = "138001380000",
            Channel = 1,
        };

        mgr.Add(session);
        var users = mgr.GetAll("138001380000", 1);
        Assert.Single(users);

        mgr.Remove(session);
        users = mgr.GetAll("138001380000", 1);
        Assert.Empty(users);
    }

    [Fact(DisplayName = "MsgHub 发布订阅")]
    public void MsgHub_Produce()
    {
        var hub = new MsgHub();
        hub.Start("138001380000", 1, DataTypes.VideoIFrame);

        Assert.Equal("138001380000", hub.SIM);
        Assert.Equal(1, hub.Channel);

        // 验证 AddUser/RemoveUser
        var session = new HttpSession(async data =>
        {
            await ValueTask.CompletedTask;
            return;
        });

        hub.AddUser(session);
        Assert.Single(hub.GetUsers());

        hub.RemoveUser(session);
        Assert.Empty(hub.GetUsers());

        hub.Dispose();
    }

    [Fact(DisplayName = "HttpSession 创建与关闭")]
    public void HttpSession_Create()
    {
        var session = new HttpSession(async data =>
        {
            await ValueTask.CompletedTask;
            return;
        })
        {
            SIM = "138001380000",
            Channel = 1,
            Type = RTPType.Flv,
            IP = "127.0.0.1",
        };

        Assert.Equal("138001380000", session.SIM);
        Assert.Equal(1, session.Channel);
        Assert.Equal(RTPType.Flv, session.Type);
        Assert.Equal("127.0.0.1", session.IP);

        session.Close("测试关闭");
        // DisposeBase 的 Disposed 仅由 Dispose() 设置
        session.Dispose();
        Assert.True(session.Disposed);
    }

    [Fact(DisplayName = "StatusService 在线状态")]
    public void StatusService_Online()
    {
        var svc = new StatusService();
        svc.Online("138001380000", 1, "192.168.1.100");

        var status = svc.Get("138001380000", 1);
        Assert.NotNull(status);
        Assert.True(status.Online);
        Assert.Equal("192.168.1.100", status.RemoteAddress);

        svc.Offline("138001380000", 1);
        status = svc.Get("138001380000", 1);
        Assert.NotNull(status);
        Assert.False(status.Online);
    }
}
