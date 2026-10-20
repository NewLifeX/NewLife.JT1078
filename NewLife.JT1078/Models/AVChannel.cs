namespace NewLife.JT1078.Models;

/// <summary>通道类型</summary>
public enum AVChannelKinds : Byte
{
    /// <summary>音视频通道</summary>
    AudioVideo,
    /// <summary>纯音频通道</summary>
    Audio,
    /// <summary>纯视频通道</summary>
    Video,
}

/// <summary>音视频通道信息</summary>
public class AVChannel
{
    /// <summary>物理通道号</summary>
    public Byte PhysicalChannel { get; set; }

    /// <summary>逻辑通道号</summary>
    public Byte LogicalChannel { get; set; }

    /// <summary>通道类型</summary>
    public AVChannelKinds ChannelType { get; set; }

    /// <summary>是否连接云台</summary>
    /// <remarks>
    /// 通道类型为 0 和 2 时，此字段有效；0：未连接；1：连接
    /// </remarks>
    public Byte HasPanTilt { get; set; }
}