using NewLife.Caching;
using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 TCP 服务端</summary>
/// <remarks>
/// 监听 TCP 端口 1078，接收车载终端上传的 RTP 音视频流数据。
/// 自动处理 TCP 粘包和 RTP 分包合并。
/// </remarks>
public class JT1078Server : NetServer<JT1078Session>
{
    /// <summary>JT1078 配置</summary>
    public JT1078Setting Setting { get; set; } = new();

    /// <summary>分包缓存。Key=SIM-Channel-AVType，用于合并 RTP 分包</summary>
    public ICache Cache { get; set; } = MemoryCache.Instance;

    /// <summary>服务提供者。用于创建依赖注入作用域</summary>
    public IServiceProvider ServiceProvider { get; set; }

    /// <summary>设备命令总线。支持集群广播的设备会话管理器</summary>
    public DeviceSessionManager DeviceManager { get; set; }

    /// <summary>实例化 JT1078 服务端</summary>
    public JT1078Server()
    {
        Name = "JT1078";
        Port = 1078;

        // 仅监听 TCP，UDP 由 JT1078UdpServer 独立承载，避免 NetServer 默认双栈占用 UDP 端口
        ProtocolType = NetType.Tcp;

        // 启用协议模式：RTP 报文的定界与构建由 JTCodec 承担
        Protocol = new JTCodec();
    }

    /// <summary>启动服务</summary>
    protected override void OnStart()
    {
        if (Setting.Debug) Log = XTrace.Log;

        Port = Setting.Port > 0 ? Setting.Port : 1078;

        // 初始化设备命令总线
        DeviceManager ??= new DeviceSessionManager(ServiceProvider);
        DeviceManager.Init();

        base.OnStart();

        WriteLog("JT1078 server started on port {0}", Port);
    }

    /// <summary>停止服务</summary>
    /// <param name="reason">停止原因</param>
    protected override void OnStop(String reason)
    {
        // 释放命令总线并置空，便于重启时重新初始化事件订阅
        DeviceManager?.Dispose();
        DeviceManager = null;

        base.OnStop(reason);
    }

    /// <summary>RTP 分包合并</summary>
    /// <remarks>
    /// 按 {SIM}-{Channel}-{AVType} 缓存首包，
    /// 收到末包后拼接完整帧并返回。
    /// </remarks>
    /// <param name="session">会话</param>
    /// <param name="msg">RTP 报文</param>
    /// <returns>完整帧（原子包或合并完成），未完成时返回 null</returns>
    public virtual JTMessage? Merge(JT1078Session session, JTMessage msg)
    {
        if (msg == null) return null;

        if (msg.PacketFlag == PacketFlags.Alone) return msg;

        // 无 SIM 无法按设备聚合分包，直接按独立包处理
        if (msg.SIM.IsNullOrEmpty()) return msg;

        // 按 SIM+通道+编码类型 作为分包键，跨连接稳定；避免依赖底层 Socket 会话
        var key = $"{msg.SIM}-{msg.Channel}-{msg.Type}";

        if (msg.PacketFlag == PacketFlags.First)
        {
            // 首包：缓存起来，等待后续包。协议模式下消息在处理器返回后即收尾，必须克隆消息留存
            Cache.Set(key, msg.Clone(), 600);
            return null;
        }

        // 中间包或末包
        var first = Cache.Get<JTMessage>(key);
        if (first == null)
        {
            // 缺少首包，丢弃残缺帧，避免向下游输出不完整数据
            return null;
        }

        first.Append(msg);

        if (msg.PacketFlag == PacketFlags.Last)
        {
            // 末包：合并完成，清除缓存
            Cache.Remove(key);
            // 链式分段无法被 GetSpan 一次性读取，合并为连续缓冲区，保证音视频解码完整
            if (first.Payload?.Next != null) first.Payload = new ArrayPacket(first.Payload.ToArray());
            return first;
        }

        // 中间包：更新缓存
        Cache.Set(key, first, 600);
        return null;
    }
}
