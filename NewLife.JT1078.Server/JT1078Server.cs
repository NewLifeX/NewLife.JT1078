using NewLife.Caching;
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

    /// <summary>分包缓存。Key=SessionId-Channel-AVType，用于合并 RTP 分包</summary>
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

        // 添加粘包编解码器
        Add(new JTCodec());
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

        WriteLog("JT1078 Server started on port {0}", Port);
    }

    /// <summary>RTP 分包合并</summary>
    /// <remarks>
    /// 按 {SessionId}-{Channel}-{AVType} 缓存中间包，
    /// 收到末包后拼接完整帧并返回。
    /// </remarks>
    /// <param name="session">会话</param>
    /// <param name="msg">RTP 报文</param>
    /// <returns>完整帧（原子包或合并完成），未完成时返回 null</returns>
    public virtual JTMessage? Merge(JT1078Session session, JTMessage msg)
    {
        if (msg.PacketFlag == PacketFlags.Alone) return msg;

        var key = $"{session.Remote}-{msg.Channel}-{msg.Type}";

        if (msg.PacketFlag == PacketFlags.First)
        {
            // 首包：缓存起来，等待后续包
            Cache.Set(key, msg, 600);
            return null;
        }

        // 中间包或末包
        var first = Cache.Get<JTMessage>(key);
        if (first == null) return msg; // 缓存丢失，当作独立包处理

        first.Append(msg);

        if (msg.PacketFlag == PacketFlags.Last)
        {
            // 末包：合并完成，清除缓存
            Cache.Remove(key);
            return first;
        }

        // 中间包：更新缓存
        Cache.Set(key, first, 600);
        return null;
    }
}
