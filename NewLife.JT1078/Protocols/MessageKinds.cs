namespace NewLife.JT1078.Protocols;

/// <summary>消息类型</summary>
public enum MessageKinds : UInt16
{
    /// <summary>查询终端音视频属性</summary>
    QueryAVAttribute = 0x9003,

    /// <summary>终端上传音视频属性</summary>
    UploadAVAttribute = 0x1003,

    /// <summary>终端上传乘客流量</summary>
    UploadPassengerFlow = 0x1005,

    /// <summary>实时音视频传输请求</summary>
    AVTransmitRequest = 0x9101,

    /// <summary>音视频实时传输控制</summary>
    AVTransmitControl = 0x9102,

    /// <summary>实时音视频传输状态通知</summary>
    AVStatusNotify = 0x9105,

    /// <summary>查询资源列表</summary>
    QueryResourceList = 0x9205,

    /// <summary>文件上传指令</summary>
    FileUploadCommand = 0x9206,

    /// <summary>文件上传控制</summary>
    FileUploadControl = 0x9207,

    /// <summary>终端上传音视频资源列表</summary>
    AVResourceList = 0x1205,

    /// <summary>文件上传完成通知</summary>
    FileUploadComplete = 0x1206,

    /// <summary>平台下发远程录像回放请求</summary>
    PlaybackRequest = 0x9201,

    /// <summary>平台下发远程录像回放控制</summary>
    PlaybackControl = 0x9202,
}