namespace NewLife.JT1078.Models
{
    /// <summary>扩展视频相关报警</summary>
    [Flags]
    public enum VideoAlarmKinds : UInt32
    {
        视频信号丢失 = 1 << 0,
        视频信号阻挡 = 1 << 1,
        存储单元故障 = 1 << 2,
        其它视频设备故障 = 1 << 3,
        客车超员 = 1 << 4,
        异常驾驶行为 = 1 << 5,
        特殊报警录像满 = 1 << 6,
    }
}