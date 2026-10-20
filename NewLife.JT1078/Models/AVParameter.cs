namespace NewLife.JT1078.Models;

/// <summary></summary>
public enum AVCodecModes : Byte
{
    /// <summary>固定码率</summary>
    CBR = 0,
    /// <summary>可变码率</summary>
    VBR = 1,
    /// <summary>平均码率</summary>
    ABR = 2,
}

/// <summary></summary>
public enum AVCodecRates : Byte
{
    /// <summary>176×144</summary>
    QCIF = 0,
    /// <summary>352×288</summary>
    CIF = 1,
    /// <summary>480×288</summary>
    WCIF = 2,
    /// <summary>704×576</summary>
    D1 = 3,
    /// <summary>960×576</summary>
    WD1 = 4,
    P720 = 5,
    P1080 = 6,
}

/// <summary></summary>
[Flags]
public enum OSDFlags : UInt16
{
    DateTime = 1 << 0,
    PlateNumber = 1 << 1,
    LogicalChannel = 1 << 2,
    LongitudeLatitude = 1 << 3,
    DrivingSpeed = 1 << 4,
    GpsSpeed = 1 << 5,
    ContinuousDrivingTime = 1 << 6,
}

/// <summary>音视频参数</summary>
public class AVParameter
{
    #region 属性
    /// <summary>实时流编码模式</summary>
    /// <remarks>
    /// 0：CBR（固定码率）；
    /// 1：VBR（可变码率）；
    /// 2：ABR（平均码率）；
    /// 100～127：自定义
    /// </remarks>
    public AVCodecModes RealtimeCodecMode { get; set; }

    /// <summary>实时流分辨率</summary>
    /// <remarks>
    /// 0：QCIF；
    /// 1：CIF；
    /// 2：WCIF；
    /// 3：D1；
    /// 4：WD1；
    /// 5：720P；
    /// 6：1080P；
    /// 100～127：自定义
    /// </remarks>
    public AVCodecRates RealtimeResolution { get; set; }

    /// <summary>实时流关键帧间隔</summary>
    /// <remarks>范围 1～1000 帧</remarks>
    public UInt16 RealtimeKeyFrameInterval { get; set; }

    /// <summary>实时流目标帧率</summary>
    /// <remarks>范围（1～120）帧/秒</remarks>
    public Byte RealtimeTargetFps { get; set; }

    /// <summary>实时流目标码率</summary>
    /// <remarks>单位为千位每秒（kbps）</remarks>
    public UInt32 RealtimeTargetBitrate { get; set; }

    /// <summary>存储流编码模式</summary>
    public AVCodecModes StorageCodecMode { get; set; }

    /// <summary>存储流分辨率</summary>
    public AVCodecRates StorageResolution { get; set; }

    /// <summary>存储流关键帧间隔</summary>
    /// <remarks>范围 1～1000 帧</remarks>
    public UInt16 StorageKeyFrameInterval { get; set; }

    /// <summary>存储流目标帧率</summary>
    /// <remarks>范围（1～120）帧/秒</remarks>
    public Byte StorageTargetFps { get; set; }

    /// <summary>存储流目标码率</summary>
    /// <remarks>单位为千位每秒（kbps）</remarks>
    public UInt32 StorageTargetBitrate { get; set; }

    /// <summary>OSD字幕叠加设置</summary>
    public OSDFlags OSDSettings { get; set; }

    /// <summary>是否启用音频输出</summary>
    public Byte AudioOutputEnabled { get; set; }
    #endregion
}