namespace NewLife.JT1078.Client.Models;

/// <summary>消息日志条目</summary>
public class MessageLogEntry
{
    /// <summary>时间戳</summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>方向。发送/接收</summary>
    public String Direction { get; set; } = "";

    /// <summary>消息摘要</summary>
    public String Summary { get; set; } = "";

    /// <summary>原始数据（十六进制）</summary>
    public String? HexData { get; set; }

    /// <summary>消息类型</summary>
    public String? MessageType { get; set; }

    /// <summary>日志级别</summary>
    public String Level { get; set; } = "Info";
}
