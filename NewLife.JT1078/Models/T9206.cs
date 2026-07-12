using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;

namespace NewLife.JT1078.Models;

/// <summary>文件上传指令</summary>
[MessageKind(MessageKinds.FileUploadCommand)]
public class T9206
{
    /// <summary>FTP 服务器 IP 地址</summary>
    public String Server { get; set; }

    /// <summary>FTP 服务器端口</summary>
    public UInt16 Port { get; set; }

    /// <summary>FTP 用户名</summary>
    public String UserName { get; set; }

    /// <summary>FTP 密码</summary>
    public String Password { get; set; }

    /// <summary>文件上传路径</summary>
    public String Path { get; set; }

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

    /// <summary>执行条件</summary>
    public Byte Condition { get; set; }
}
