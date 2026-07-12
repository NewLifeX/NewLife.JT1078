using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Protocols;

/// <summary>消息处理接口。每个格式一个实现，瞬态注册</summary>
public interface IMsgHandler
{
    /// <summary>所属消息中枢</summary>
    IMsgHub Hub { get; set; }

    /// <summary>编码消息并推送给订阅者</summary>
    /// <param name="message">JT1078 RTP 报文</param>
    /// <param name="users">匹配的订阅者列表</param>
    /// <returns>推送的订阅者数量</returns>
    Task<Int32> EncodeAsync(JTMessage message, IMsgUser[] users);
}
