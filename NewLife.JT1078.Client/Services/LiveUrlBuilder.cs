using NewLife.JT1078.Client.Models;

namespace NewLife.JT1078.Client.Services;

/// <summary>直播拉流地址构建器</summary>
public static class LiveUrlBuilder
{
    /// <summary>判断指定格式是否可由内置播放器（LibVLC）渲染</summary>
    /// <param name="format">流格式，如 FLV/fMP4/HLS/AAC/WAV/MP3</param>
    /// <returns>支持播放时返回 true；PCM 等裸流格式返回 false</returns>
    public static Boolean IsPlayable(String? format)
    {
        var fmt = Normalize(format);

        return fmt is "flv" or "mp4" or "hls" or "aac" or "wav" or "mp3";
    }

    /// <summary>构建 HTTP 直播拉流地址</summary>
    /// <param name="config">服务器连接配置</param>
    /// <param name="format">流格式，fmp4 会映射为 mp4</param>
    /// <param name="sim">终端 SIM 卡号</param>
    /// <param name="channel">逻辑通道号，从 1 开始</param>
    /// <returns>完整的 HTTP 拉流地址</returns>
    /// <exception cref="ArgumentNullException">config 为空时抛出</exception>
    /// <exception cref="ArgumentException">sim 为空时抛出</exception>
    /// <exception cref="ArgumentOutOfRangeException">channel 小于 1 时抛出</exception>
    public static String BuildHttpUrl(ConnectionConfig config, String format, String sim, Int32 channel)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (String.IsNullOrWhiteSpace(sim)) throw new ArgumentException("SIM 卡号不能为空", nameof(sim));
        if (channel < 1) throw new ArgumentOutOfRangeException(nameof(channel), "逻辑通道号必须大于 0");

        var fmt = Normalize(format);
        var path = fmt == "hls" ? "hls.m3u8" : $"live.{fmt}";

        var url = $"{config.HttpBaseUrl.TrimEnd('/')}/{path}?sim={Uri.EscapeDataString(sim.Trim())}&channel={channel}";
        if (!String.IsNullOrWhiteSpace(config.Token))
            url += $"&token={Uri.EscapeDataString(config.Token.Trim())}";

        return url;
    }

    private static String Normalize(String? format)
    {
        var fmt = format?.Trim().ToLowerInvariant() ?? String.Empty;

        return fmt == "fmp4" ? "mp4" : fmt;
    }
}
