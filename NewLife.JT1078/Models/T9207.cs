using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Models;

/// <summary>文件上传控制</summary>
[MessageKind(MessageKinds.FileUploadControl)]
public class T9207
{
    /// <summary>流水号</summary>
    public Int16 SerialNumber { get; set; }

    /// <summary>控制指令。0：继续上传；1：暂停上传；2：取消上传</summary>
    public Byte Control { get; set; }
}
