using NewLife;
using NewLife.JT1078.Models;

namespace NewLife.JT1078.Server;

/// <summary>消息用户接口。代表一个拉流订阅者（HTTP/WS）</summary>
public interface IMsgUser : IDisposable2
{
    /// <summary>会话标识</summary>
    Int32 Id { get; }

    /// <summary>业务编码</summary>
    String BizCode { get; set; }

    /// <summary>SIM 卡号</summary>
    String SIM { get; set; }

    /// <summary>通道号</summary>
    Int32 Channel { get; set; }

    /// <summary>用户标识</summary>
    Int32 UserId { get; set; }

    /// <summary>用户名</summary>
    String UserName { get; set; }

    /// <summary>是否已开始推送</summary>
    Boolean HasStarted { get; set; }

    /// <summary>客户端 IP</summary>
    String IP { get; set; }

    /// <summary>最后活跃时间</summary>
    DateTime LastActive { get; set; }

    /// <summary>播放模式</summary>
    String Mode { get; set; }

    /// <summary>所属消息中枢</summary>
    IMsgHub Hub { get; set; }

    /// <summary>是否仅音频</summary>
    Boolean OnlyAudio { get; set; }

    /// <summary>是否需要音频</summary>
    Boolean RequireAudio { get; set; }

    /// <summary>是否需要视频</summary>
    Boolean RequireVideo { get; set; }

    /// <summary>开始时间</summary>
    DateTime StartTime { get; set; }

    /// <summary>流输出类型</summary>
    RTPType Type { get; set; }

    /// <summary>关闭连接</summary>
    void Close(String reason);

    /// <summary>分离消息中枢</summary>
    void Detach(IMsgHub hub);

    /// <summary>写入数据</summary>
    ValueTask WriteAsync(Byte[] data);
}
