using NewLife.JT1078.Flv.Enums;
using NewLife.JT1078.Flv.Metadata;

namespace NewLife.JT1078.Flv;

/// <summary>FLV标签数据</summary>
/// <remarks>
/// Tag有三种类型，Audio Tag（音频Tag），Video Tag（视频Tag），script Tag（又称Metadata Tag）
/// </remarks>
public class FlvTags
{
    /// <summary>标签类型</summary>
    public TagType Type { get; set; }

    /// <summary>
    /// Tag Data部分大小
    /// 3个字节
    /// </summary>
    public Int32 DataSize { get; set; }

    /// <summary>
    /// Tag时间戳
    /// 3个字节
    /// </summary>
    public UInt32 Timestamp { get; set; }

    public Byte TimestampExt { get; set; }

    /// <summary>
    /// stream id 总是0
    /// 3个字节
    /// </summary>
    public UInt32 StreamId { get; set; }

    /// <summary>视频数据</summary>
    public VideoTags VideoTagsData { get; set; }

    /// <summary>音频数据</summary>
    public AudioTags AudioTagsData { get; set; }

    /// <summary>脚本数据</summary>
    public Amf3 DataTagsData { get; set; }
}