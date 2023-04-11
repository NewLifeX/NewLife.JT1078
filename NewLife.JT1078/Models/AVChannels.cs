namespace NewLife.JT1078.Models
{
    public class AVChannels
    {
        public Byte 音视频通道总数 { get; set; }

        public Byte 音频通道总数 { get; set; }

        public Byte 视频通道总数 { get; set; }

        public AVChannel[] Channels { get; set; }
    }
}