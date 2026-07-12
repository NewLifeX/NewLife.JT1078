using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 会话</summary>
/// <remarks>每个 TCP 连接对应一个会话实例，管理终端设备和数据收发</remarks>
public class JT1078Session : NetSession<JT1078Server>
{
    #region 属性
    /// <summary>最后活跃时间</summary>
    public DateTime LastActive { get; set; } = DateTime.Now;

    /// <summary>最后推送时间。用于判断是否无人订阅</summary>
    private DateTime _lastPush = DateTime.Now;

    /// <summary>空闲超时秒数。超过此时间无推送则自动断开</summary>
    public Int32 IdleTime { get; set; } = 30;

    /// <summary>消息序号生成器</summary>
    private Int32 _gid;

    /// <summary>服务提供者。可用于创建子作用域</summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>接收到 RTP 报文时触发</summary>
    public event EventHandler<JTMessage> OnReceiveMessage;
    #endregion

    #region 方法
    /// <summary>接收数据</summary>
    /// <param name="e"></param>
    protected override void OnReceive(ReceivedEventArgs e)
    {
        LastActive = DateTime.Now;

        var rawMsg = e.Message as JTMessage;
        var msg = Host?.Merge(this, rawMsg);
        if (msg != null)
        {
            OnMessage(msg);
        }
        else if (rawMsg == null)
        {
            WriteLog("NoMessage {0}", e.Packet);
        }

        base.OnReceive(e);

        // 空闲超时自动断开
        var exp = (Int32)(DateTime.Now - _lastPush).TotalSeconds;
        if (!Disposed && exp > IdleTime)
        {
            WriteLog("当前通道无人订阅超时（{0}>{1}秒），准备关闭", exp, IdleTime);
            Dispose();
        }
    }

    /// <summary>接收到完整消息</summary>
    /// <param name="msg"></param>
    protected virtual void OnMessage(JTMessage msg)
    {
        OnReceiveMessage?.Invoke(this, msg);
    }

    /// <summary>向终端发送下行信令</summary>
    /// <param name="avtype">音视频编码类型</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="payload">负载数据</param>
    /// <returns>发送的字节数</returns>
    public virtual Int32 SendJT(AVTypes avtype, DataTypes dataType, Packet payload)
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = avtype,
            Completed = true,
            Sequence = (UInt16)Interlocked.Increment(ref _gid),
            Channel = 0,
            DataType = dataType,
            PacketFlag = PacketFlags.Alone,
            Timestamp = (UInt64)DateTime.Now.ToLong(),
            Payload = payload,
        };

        return SendMessage(msg);
    }

    /// <summary>更新最后推送时间</summary>
    public void TouchPush() => _lastPush = DateTime.Now;
    #endregion
}
