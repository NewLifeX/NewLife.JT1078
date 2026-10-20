namespace NewLife.JT1078.Models;

/// <summary>扩展视频相关报警</summary>
[Flags]
public enum VideoAlarmKinds : UInt32
{
    /// <summary>视频信号丢失</summary>
    VideoSignalLoss = 1 << 0,

    /// <summary>视频信号阻挡</summary>
    VideoSignalBlocked = 1 << 1,

    /// <summary>存储单元故障</summary>
    StorageUnitFailure = 1 << 2,

    /// <summary>其它视频设备故障</summary>
    OtherVideoDeviceFailure = 1 << 3,

    /// <summary>客车超员</summary>
    PassengerOverload = 1 << 4,

    /// <summary>异常驾驶行为</summary>
    AbnormalDriving = 1 << 5,

    /// <summary>特殊报警录像满</summary>
    SpecialAlarmRecordingFull = 1 << 6,
}