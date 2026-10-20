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

    /// <summary>设备标识。SIM+Channel 构成的唯一键</summary>
    public String DeviceKey { get; set; }

    /// <summary>终端 SIM 卡号。由上行报文自动记录</summary>
    public String SIM { get; set; }

    /// <summary>逻辑通道号。由上行报文自动记录</summary>
    public Int32 Channel { get; set; }

    /// <summary>接收到 RTP 报文时触发</summary>
    public event EventHandler<JTMessage> OnReceiveMessage;
    #endregion

    #region 构造
    /// <summary>释放会话时注销设备注册</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        if (DeviceKey != null)
        {
            var mgr = Host?.DeviceManager;
            mgr?.Unregister(DeviceKey);
        }

        // 设备下线时清理消息中枢，避免订阅者与缓存泄漏
        if (!SIM.IsNullOrEmpty()) MsgHubRegistry.Remove(SIM, Channel);
    }
    #endregion

    #region 方法
    /// <summary>接收数据</summary>
    /// <param name="e"></param>
    protected override void OnReceive(ReceivedEventArgs e)
    {
        LastActive = DateTime.Now;

        var rawMsg = e.Message as JTMessage;

        // 头部先行交付时消息体为流式读取器，处理器返回前必须读满物化
        if (rawMsg != null && rawMsg.Payload == null && rawMsg.Body != null)
            rawMsg.Payload = rawMsg.Body.ReadAllAsync().AsTask().GetAwaiter().GetResult();

        if (rawMsg != null && !rawMsg.SIM.IsNullOrEmpty())
        {
            SIM = rawMsg.SIM;
            Channel = rawMsg.Channel;

            var key = rawMsg.GetKey();
            if (DeviceKey != key)
            {
                // 通道变化时先注销旧键，避免设备字典残留
                if (DeviceKey != null) Host?.DeviceManager?.Unregister(DeviceKey);

                DeviceKey = key;
                Host?.DeviceManager?.Register(DeviceKey, this);
            }
        }

        var msg = Host?.Merge(this, rawMsg);
        if (msg != null)
        {
            OnMessage(msg);
        }
        else if (rawMsg == null)
        {
            WriteLog("No message object in received data {0}", e.Packet);
        }

        base.OnReceive(e);

        // 空闲超时自动断开
        var exp = (Int32)(DateTime.Now - _lastPush).TotalSeconds;
        if (!Disposed && exp > IdleTime)
        {
            WriteLog("No subscriber for the channel for {0}>{1}s, closing", exp, IdleTime);
            Dispose();
        }
    }

    /// <summary>接收到完整消息</summary>
    /// <param name="msg"></param>
    protected virtual void OnMessage(JTMessage msg)
    {
        OnReceiveMessage?.Invoke(this, msg);

        // 分发到消息中枢，推送给拉流订阅者
        if (!msg.SIM.IsNullOrEmpty())
        {
            var hub = MsgHubRegistry.GetOrAdd(msg.SIM, msg.Channel);
            if (hub.Device == null) hub.Device = this;

            // 协议模式下消息在处理器返回后即收尾，异步分发前先物化负载
            if (hub.HasUsers)
            {
                TouchPush();
                _ = DispatchAsync(hub, msg);
            }
        }
    }

    /// <summary>异步分发消息，完成后释放克隆负载</summary>
    private static async Task DispatchAsync(MsgHub hub, JTMessage msg)
    {
        var copy = msg.Clone();
        try
        {
            await hub.ProduceAsync(copy).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }
        finally
        {
            copy.TryDispose();
        }
    }

    /// <summary>向终端发送下行信令</summary>
    /// <param name="avtype">音视频编码类型</param>
    /// <param name="dataType">数据类型</param>
    /// <param name="payload">负载数据</param>
    /// <returns>发送的字节数</returns>
    public virtual Int32 SendJT(AVTypes avtype, DataTypes dataType, IPacket payload)
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = avtype,
            Completed = true,
            Sequence = (UInt16)Interlocked.Increment(ref _gid),
            SIM = SIM,
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
