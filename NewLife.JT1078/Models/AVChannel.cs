namespace NewLife.JT1078.Models
{
    public enum AVChannelKinds : Byte
    {
        AudioVideo,
        Audio,
        Video,
    }

    public class AVChannel
    {
        public Byte PhysicalChannel { get; set; }

        public Byte LogicalChannel { get; set; }

        public AVChannelKinds ChannelType { get; set; }

        /// <summary>是否连接云台</summary>
        /// <remarks>
        /// 通道类型为 0 和 2 时，此字段有效；0：未连接；1：连接
        /// </remarks>
        public Byte HasPanTilt { get; set; }
    }
}