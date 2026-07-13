using NewLife.Audio;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Protocols;

/// <summary>消息中枢接口。管理设备连接与用户订阅者之间的数据分发</summary>
public interface IMsgHub
{
    /// <summary>终端 SIM 卡号</summary>
    String SIM { get; set; }

    /// <summary>逻辑通道号</summary>
    Int32 Channel { get; set; }

    /// <summary>设备会话对象（可由不同传输层实现）</summary>
    Object Device { get; set; }

    /// <summary>空闲超时秒数。超过此时间无推送则自动断开</summary>
    Int32 IdleTime { get; set; }

    /// <summary>音频编解码器工厂</summary>
    AudioCodecFactory AudioCodec { get; }

    /// <summary>开始处理指定通道的数据</summary>
    void Start(String sim, Int32 channel, DataTypes type);

    /// <summary>关闭通道</summary>
    void Close(String reason);

    /// <summary>生产消息，分发给所有订阅者</summary>
    Task<Int32> ProduceAsync(JTMessage msg, CancellationToken cancellationToken = default);

    /// <summary>向所有订阅者写入数据</summary>
    ValueTask WriteAsync(Byte[] data);

    /// <summary>写入文件缓存</summary>
    void WriteFile(String type, Byte[] data, Int32 offset = 0, Int32 count = -1);

    /// <summary>获取文件缓存流</summary>
    Stream GetFile(String type);

    /// <summary>添加订阅者</summary>
    Boolean AddUser(IMsgUser user);

    /// <summary>移除订阅者</summary>
    Boolean RemoveUser(IMsgUser user);

    /// <summary>获取所有订阅者</summary>
    IMsgUser[] GetUsers();

    /// <summary>写日志</summary>
    void WriteLog(String format, params Object[] args);
}
