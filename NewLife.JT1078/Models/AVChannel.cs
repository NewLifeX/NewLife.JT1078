namespace NewLife.JT1078.Models
{
    public enum AVChannelKinds : Byte
    {
        音视频,
        音频,
        视频,
    }

    public class AVChannel
    {
        public Byte 物理通道号 { get; set; }

        public Byte 逻辑通道号 { get; set; }

        public AVChannelKinds 通道类型 { get; set; }

        /// <summary>是否连接云台</summary>
        /// <remarks>
        /// 通道类型为 0 和 2 时，此字段有效；0：未连接；1：连接
        /// </remarks>
        public Byte 是否连接云台 { get; set; }
    }
}