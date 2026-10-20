namespace NewLife.JT1078.Models;

/// <summary>音视频通道列表。终端上报的通道配置信息</summary>
public class AVChannels
{
    /// <summary>音视频通道总数</summary>
    public Byte TotalAVChannels { get; set; }

    /// <summary>音频通道总数</summary>
    public Byte TotalAudioChannels { get; set; }

    /// <summary>视频通道总数</summary>
    public Byte TotalVideoChannels { get; set; }

    /// <summary>通道详情列表</summary>
    public AVChannel[] Channels { get; set; }
}