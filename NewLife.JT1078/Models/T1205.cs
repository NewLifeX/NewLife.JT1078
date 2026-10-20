using System.Globalization;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;
using NewLife.Serialization;

namespace NewLife.JT1078.Models;

/// <summary>终端上传音视频资源列表</summary>
/// <remarks>消息体格式：流水号(2) + 资源总数(4) + N × 资源项(28)</remarks>
[MessageKind(MessageKinds.AVResourceList)]
public class T1205 : IAccessor
{
    private const Int32 ItemSize = 28;
    private const String TimeFormat = "yyMMddHHmmss";

    /// <summary>流水号</summary>
    public Int16 SerialNumber { get; set; }

    /// <summary>资源列表</summary>
    public List<T1205ResourceInfo> Items { get; set; } = [];

    /// <summary>读取资源列表</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns>是否成功</returns>
    public Boolean Read(Stream stream, Object context)
    {
        if (stream == null) return false;

        // 头部不足流水号与资源总数时无法解析
        if (stream.Length - stream.Position < 6) return false;

        var reader = new Binary { Stream = stream, IsLittleEndian = false, UseRef = false };

        SerialNumber = reader.ReadInt16();
        var count = reader.ReadInt32();
        if (count < 0) return false;

        var items = new List<T1205ResourceInfo>();
        for (var i = 0; i < count; i++)
        {
            // 资源项不足时保留已解析部分，避免越界
            if (stream.Length - stream.Position < ItemSize) break;

            items.Add(new T1205ResourceInfo
            {
                Channel = reader.ReadByte(),
                StartTime = ReadBcdTime(reader),
                EndTime = ReadBcdTime(reader),
                AlarmFlags = reader.Read<UInt64>(),
                Kind = (T1205ResourceKind)reader.ReadByte(),
                CodeStream = (CodeStreams)reader.ReadByte(),
                StorageKind = (T1205StorageKind)reader.ReadByte(),
                FileSize = reader.ReadInt32(),
            });
        }
        Items = items;

        return true;
    }

    /// <summary>写入资源列表</summary>
    /// <param name="stream">数据流</param>
    /// <param name="context">上下文</param>
    /// <returns>是否成功</returns>
    public Boolean Write(Stream stream, Object context)
    {
        if (stream == null) return false;

        var writer = new Binary { Stream = stream, IsLittleEndian = false, UseRef = false };

        writer.Write(SerialNumber);

        var items = Items;
        writer.WriteInt32(items?.Count ?? 0);
        if (items != null)
        {
            foreach (var item in items)
            {
                writer.Write(item.Channel);
                writer.WriteBCD(item.StartTime.ToString(TimeFormat), 6);
                writer.WriteBCD(item.EndTime.ToString(TimeFormat), 6);
                writer.Write(item.AlarmFlags);
                writer.Write((Byte)item.Kind);
                writer.Write((Byte)item.CodeStream);
                writer.Write((Byte)item.StorageKind);
                writer.WriteInt32(item.FileSize);
            }
        }

        return true;
    }

    private static DateTime ReadBcdTime(Binary reader)
    {
        var str = reader.ReadBCD(6);

        return DateTime.TryParseExact(str, TimeFormat, null, DateTimeStyles.None, out var dt) ? dt : DateTime.MinValue;
    }
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
