using System.Text;

namespace NewLife.JT1078.Server.Hls;

/// <summary>HLS M3U8 播放列表生成器</summary>
internal static class M3U8Writer
{
    /// <summary>生成 M3U8 播放列表</summary>
    /// <param name="segments">段信息列表</param>
    /// <param name="targetDuration">目标段时长（秒）</param>
    /// <returns>M3U8 文本</returns>
    public static String WritePlaylist(IList<HlsSegmentInfo> segments, Double targetDuration)
    {
        var sb = new StringBuilder();
        sb.AppendLine("#EXTM3U");
        sb.AppendLine("#EXT-X-VERSION:3");
        sb.AppendLine("#EXT-X-TARGETDURATION:" + (Int32)Math.Ceiling(targetDuration));
        sb.AppendLine("#EXT-X-MEDIA-SEQUENCE:" + (segments.Count > 0 ? segments[0].Sequence : 0));
        sb.AppendLine("#EXT-X-DISCONTINUITY");

        foreach (var seg in segments)
        {
            sb.AppendLine("#EXTINF:" + seg.Duration.ToString("F3") + ",");
            sb.AppendLine(seg.Url);
        }

        return sb.ToString();
    }
}

/// <summary>HLS 段信息</summary>
public class HlsSegmentInfo
{
    /// <summary>段序号</summary>
    public Int32 Sequence { get; set; }

    /// <summary>段时长（秒）</summary>
    public Double Duration { get; set; }

    /// <summary>段 URL</summary>
    public String Url { get; set; } = String.Empty;

    /// <summary>段数据</summary>
    public Byte[] Data { get; set; } = [];
}
