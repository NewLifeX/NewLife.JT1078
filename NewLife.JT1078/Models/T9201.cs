using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;

namespace NewLife.JT1078.Models
{
    /// <summary>平台下发远程录像回放请求</summary>
    [MessageKind(MessageKinds.PlaybackRequest)]
    public class T9201
    {
        /// <summary>服务器IP</summary>
        public String Server { get; set; }

        /// <summary>Tcp端口</summary>
        public UInt16 TcpPort { get; set; }

        /// <summary>Udp端口</summary>
        public UInt16 UdpPort { get; set; }

        /// <summary>逻辑通道号</summary>
        public Byte Channel { get; set; }

        /// <summary>音视频资源类型</summary>
        public MediaFileKinds FileKind { get; set; }

        /// <summary>码流类型</summary>
        public CodeStreams CodeStream { get; set; }

        /// <summary>存储器类型</summary>
        public StorageKinds StorageKind { get; set; }

        /// <summary>回放方式</summary>
        /// <remarks>
        /// 0：正常回放；
        /// 1：快进回放；
        /// 2：关键帧快退回放；
        /// 3：关键帧播放；
        /// 4、单帧上传；
        /// </remarks>
        public Byte Mode { get; set; }

        /// <summary>快进或快退倍数</summary>
        /// <remarks>
        /// 回放方式为 1 和 2 时，此字段内容有效，否则置 0
        /// 0：无效；
        /// 1：1 倍；
        /// 2：2 倍；
        /// 3：4 倍；
        /// 4：8 倍；
        /// 5：16 倍
        /// </remarks>
        public Byte Multiple { get; set; }

        /// <summary>开始时间</summary>
        [BCDTime(6, "yyMMddHHmmss")]
        public DateTime StartTime { get; set; }

        /// <summary>结束时间</summary>
        [BCDTime(6, "yyMMddHHmmss")]
        public DateTime EndTime { get; set; }
    }
}