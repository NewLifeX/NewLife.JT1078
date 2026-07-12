using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Models;

/// <summary>文件上传完成通知</summary>
[MessageKind(MessageKinds.FileUploadComplete)]
public class T1206
{
    /// <summary>流水号</summary>
    public Int16 SerialNumber { get; set; }

    /// <summary>结果。0：成功；1：失败</summary>
    public Byte Result { get; set; }
}
