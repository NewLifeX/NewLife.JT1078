using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;

namespace NewLife.JT1078.Models;

/// <summary>终端上传乘客流量</summary>
[MessageKind(MessageKinds.UploadPassengerFlow)]
public class T1005
{
    /// <summary>起始时间</summary>
    [BCDTime(6, "yyMMddHHmmss")]
    public DateTime StartTime { get; set; }

    /// <summary>结束时间</summary>
    [BCDTime(6, "yyMMddHHmmss")]
    public DateTime EndTime { get; set; }

    /// <summary>上车人数</summary>
    public UInt16 GetOnCount { get; set; }

    /// <summary>下车人数</summary>
    public UInt16 GetOffCount { get; set; }
}
