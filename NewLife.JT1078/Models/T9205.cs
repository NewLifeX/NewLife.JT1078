using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;

namespace NewLife.JT1078.Models;

/// <summary>查询资源列表</summary>
[MessageKind(MessageKinds.QueryResourceList)]
public class T9205
{
    /// <summary>逻辑通道号</summary>
    public Byte Channel { get; set; }

    /// <summary>开始时间</summary>
    [BCDTime(6, "yyMMddHHmmss")]
    public DateTime StartTime { get; set; }

    /// <summary>结束时间</summary>
    [BCDTime(6, "yyMMddHHmmss")]
    public DateTime EndTime { get; set; }

    /// <summary>报警标志</summary>
    public UInt64 AlarmFlags { get; set; }

    /// <summary>音视频资源类型</summary>
    public MediaFileKinds FileKind { get; set; }

    /// <summary>码流类型</summary>
    public CodeStreams CodeStream { get; set; }

    /// <summary>存储器类型</summary>
    public StorageKinds StorageKind { get; set; }
}