namespace NewLife.JT1078.Web;

/// <summary>直播查询参数</summary>
public class LiveModel
{
    /// <summary>SIM 卡号</summary>
    public String SIM { get; set; }

    /// <summary>逻辑通道号</summary>
    public Int32 Channel { get; set; } = 1;

    /// <summary>设备认证 Token</summary>
    public String Token { get; set; }

    /// <summary>推流模式。flv/mp4/aac/pcm/wav</summary>
    public String Mode { get; set; }
}
