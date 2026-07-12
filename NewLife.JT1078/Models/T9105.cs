using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Models;

/// <summary>实时音视频传输状态通知</summary>
[MessageKind(MessageKinds.AVStatusNotify)]
public class T9105
{
    /// <summary>逻辑通道号</summary>
    public Byte Channel { get; set; }

    /// <summary>丢包率（百分比）</summary>
    public Byte LossRate { get; set; }

    /// <summary>误码率（百分比）</summary>
    public Byte ErrorRate { get; set; }
}
