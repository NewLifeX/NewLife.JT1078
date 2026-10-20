using NewLife.JT1078.Protocols;
using NewLife.Video.Codecs;

namespace NewLife.JT1078.H264;

public class H264NALU
{
    public static readonly Byte[] Start1 = new Byte[3] { 0, 0, 1 };
    public static readonly Byte[] Start2 = new Byte[4] { 0, 0, 0, 1 };

    /// <summary></summary>
    public Byte[] StartCodePrefix { get; set; }

    /// <summary></summary>
    public NALUHeader NALUHeader { get; set; }

    /// <summary>
    /// 终端设备SIM卡号
    /// BCD[6]
    /// </summary>
    public String SIM { get; set; }

    /// <summary>逻辑通道号</summary>
    public Byte Channel { get; set; }

    /// <summary>数据类型</summary>
    public DataTypes DataType { get; set; }

    /// <summary>
    /// 该帧与上一个关键帧之间的时间间隔，单位毫秒(ms),
    /// 当数据类型为非视频帧时，则没有该字段
    /// </summary>
    public UInt16 LastIFrameInterval { get; set; }

    /// <summary>
    /// 该帧与上一个帧之间的时间间隔，单位毫秒(ms),
    /// 当数据类型为非视频帧时，则没有该字段
    /// </summary>
    public UInt16 LastFrameInterval { get; set; }

    /// <summary>
    /// 时间戳
    /// 标识此RTP数据包当前帧的相对时间，单位毫秒（ms）。
    /// 当数据类型为01000时，则没有该字段
    /// </summary>
    public UInt64 Timestamp { get; set; }

    /// <summary>数据体</summary>
    public Byte[] RawData { get; set; }

    public String GetAVKey() => $"{SIM}_{Channel}";
}