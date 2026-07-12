using NewLife.JT1078.Protocols;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 会话</summary>
/// <remarks>每个 TCP 连接对应一个会话实例，管理终端设备和数据收发</remarks>
public class JT1078Session : NetSession<JT1078Server>
{
    /// <summary>最后活跃时间</summary>
    public DateTime LastActive { get; set; } = DateTime.Now;

    /// <summary>接收到 RTP 报文时触发</summary>
    public event EventHandler<JTMessage>? OnReceiveMessage;

    /// <summary>接收数据</summary>
    /// <param name="e"></param>
    protected override void OnReceive(ReceivedEventArgs e)
    {
        base.OnReceive(e);

        LastActive = DateTime.Now;
    }

    /// <summary>接收到完整消息</summary>
    /// <param name="msg"></param>
    protected virtual void OnMessage(JTMessage msg)
    {
        // 分包合并
        var merged = Host?.Merge(this, msg);
        if (merged == null) return;

        // 触发事件
        OnReceiveMessage?.Invoke(this, merged);
    }
}
