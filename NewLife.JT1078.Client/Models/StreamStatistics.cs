namespace NewLife.JT1078.Client.Models;

/// <summary>流媒体统计信息</summary>
public class StreamStatistics
{
    /// <summary>已接收字节数</summary>
    public Int64 ReceivedBytes { get; set; }

    /// <summary>已接收包数</summary>
    public Int64 ReceivedPackets { get; set; }

    /// <summary>当前码率（bps）</summary>
    public Double Bitrate { get; set; }

    /// <summary>持续时间</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>开始时间</summary>
    public DateTime StartTime { get; set; } = DateTime.Now;
}
