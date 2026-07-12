
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Models
{
    /// <summary>实时音视频传输请求</summary>
    [MessageKind(MessageKinds.AVTransmitRequest)]
    public class T9101
    {
        /// <summary>服务器IP</summary>
        public String Server { get; set; }

        /// <summary>Tcp端口</summary>
        public UInt16 TcpPort { get; set; }

        /// <summary>Udp端口</summary>
        public UInt16 UdpPort { get; set; }

        /// <summary>逻辑通道号</summary>
        public Byte Channel { get; set; }

        /// <summary>数据类型</summary>
        public MediaKinds MediaKind { get; set; }

        /// <summary>码流类型</summary>
        public CodeStreams CodeStream { get; set; }
    }
}