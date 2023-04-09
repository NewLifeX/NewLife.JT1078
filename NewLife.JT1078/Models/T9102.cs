using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Models
{
    /// <summary>音视频实时传输控制</summary>
    [MessageKind(MessageKinds.音视频传输控制)]
    public class T9102
    {
        /// <summary>逻辑通道号</summary>
        public Byte Channel { get; set; }

        /// <summary>控制指令</summary>
        /// <remarks>
        /// 平台可以通过该指令对设备的实时音视频进行控制。
        /// 0：关闭音视频传输指令
        /// 1：切换码流（增加暂停和继续）
        /// 2：暂停该通道所有流的发送
        /// 3：恢复暂停前流的发送，与暂停前的流类型一致
        /// 4：关闭双向对讲
        /// </remarks>
        public StreamCommands Command { get; set; }

        /// <summary>关闭音视频类型</summary>
        /// <remarks>
        /// 0：关闭该通道有关的音视频数据
        /// 1：只关闭该通道有关的音频，保留该通道有关的视频
        /// 2：只关闭该通道有关的视频，保留该通道有关的音频
        /// </remarks>
        public CloseKinds CloseKind { get; set; }

        /// <summary>切换码流类型</summary>
        /// <remarks>
        /// 将之前申请的码流切换为新申请的码流，音频与切换前保持一致。
        /// 新申请的码流为：
        /// 0：主码流
        /// 1：子码流
        /// </remarks>
        public CodeStreams CodeStream { get; set; }
    }
}