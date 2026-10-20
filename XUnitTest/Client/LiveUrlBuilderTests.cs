using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;
using Xunit;

namespace XUnitTest.Client;

public class LiveUrlBuilderTests
{
    private static ConnectionConfig CreateConfig() => new()
    {
        Host = "192.168.1.10",
        HttpPort = 8080,
        Token = "demo",
    };

    [Fact(DisplayName = "LiveUrlBuilder 构建 FLV 直播地址")]
    public void BuildHttpUrl_Flv()
    {
        var url = LiveUrlBuilder.BuildHttpUrl(CreateConfig(), "FLV", "138001380000", 2);

        Assert.Equal("http://192.168.1.10:8080/live.flv?sim=138001380000&channel=2&token=demo", url);
    }

    [Fact(DisplayName = "LiveUrlBuilder 把 fMP4 映射为 mp4")]
    public void BuildHttpUrl_Fmp4MappedToMp4()
    {
        var url = LiveUrlBuilder.BuildHttpUrl(CreateConfig(), "fmp4", "138001380000", 1);

        Assert.Contains("/live.mp4?", url);
        Assert.DoesNotContain("fmp4", url);
    }

    [Fact(DisplayName = "LiveUrlBuilder 构建 HLS 播放列表地址")]
    public void BuildHttpUrl_Hls()
    {
        var url = LiveUrlBuilder.BuildHttpUrl(CreateConfig(), "hls", "138001380000", 1);

        Assert.Contains("/hls.m3u8?", url);
    }

    [Fact(DisplayName = "LiveUrlBuilder 无 Token 时省略参数")]
    public void BuildHttpUrl_WithoutToken()
    {
        var config = CreateConfig();
        config.Token = "  ";

        var url = LiveUrlBuilder.BuildHttpUrl(config, "flv", "138001380000", 1);

        Assert.DoesNotContain("token=", url);
    }

    [Fact(DisplayName = "LiveUrlBuilder 对 Token 做 URL 转义并修剪 SIM")]
    public void BuildHttpUrl_EscapesToken()
    {
        var config = CreateConfig();
        config.Token = "a&b=c d";

        var url = LiveUrlBuilder.BuildHttpUrl(config, "flv", " 138001380000 ", 1);

        Assert.Contains("&token=a%26b%3Dc%20d", url);
        Assert.Contains("sim=138001380000&", url);
    }

    [Fact(DisplayName = "LiveUrlBuilder 空 SIM 抛参数异常")]
    public void BuildHttpUrl_EmptySim_Throws()
    {
        Assert.Throws<ArgumentException>(() => LiveUrlBuilder.BuildHttpUrl(CreateConfig(), "flv", " ", 1));
    }

    [Fact(DisplayName = "LiveUrlBuilder 非法通道号抛越界异常")]
    public void BuildHttpUrl_InvalidChannel_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LiveUrlBuilder.BuildHttpUrl(CreateConfig(), "flv", "138001380000", 0));
    }

    [Fact(DisplayName = "LiveUrlBuilder 空配置抛空引用异常")]
    public void BuildHttpUrl_NullConfig_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => LiveUrlBuilder.BuildHttpUrl(null!, "flv", "138001380000", 1));
    }

    [Fact(DisplayName = "LiveUrlBuilder 判断可播放格式")]
    public void IsPlayable_Formats()
    {
        foreach (var format in new[] { "flv", "FLV", "fmp4", "mp4", "hls", "aac", "wav", "mp3" })
            Assert.True(LiveUrlBuilder.IsPlayable(format), format);

        foreach (var format in new String?[] { "pcm", "PCM", "unknown", "", null })
            Assert.False(LiveUrlBuilder.IsPlayable(format), format ?? "null");
    }
}
