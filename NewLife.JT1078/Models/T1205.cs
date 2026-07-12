using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;

namespace NewLife.JT1078.Models;

/// <summary>终端上传音视频资源列表</summary>
[MessageKind(MessageKinds.AVResourceList)]
public class T1205
{
    /// <summary>流水号</summary>
    public Int16 SerialNumber { get; set; }

    /// <summary>资源列表</summary>
    public List<T1205ResourceInfo> Items { get; set; } = [];
}

/// <summary>音视频资源信息</summary>
public class T1205ResourceInfo
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
    public T1205ResourceKind Kind { get; set; }

    /// <summary>码流类型</summary>
    public CodeStreams CodeStream { get; set; }

    /// <summary>存储器类型</summary>
    public T1205StorageKind StorageKind { get; set; }

    /// <summary>文件大小（字节）</summary>
    public Int32 FileSize { get; set; }
}

/// <summary>音视频资源类型（T1205）</summary>
public enum T1205ResourceKind : Byte
{
    /// <summary>音视频</summary>
    AudioVideo = 0,

    /// <summary>音频</summary>
    Audio = 1,

    /// <summary>视频</summary>
    Video = 2,
}

/// <summary>存储器类型（T1205）</summary>
public enum T1205StorageKind : Byte
{
    /// <summary>主存储器</summary>
    Main = 1,

    /// <summary>灾备存储器</summary>
    Backup = 2,
}
