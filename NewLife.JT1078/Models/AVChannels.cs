namespace NewLife.JT1078.Models
{
    public class AVChannels
    {
        public Byte TotalAVChannels { get; set; }

        public Byte TotalAudioChannels { get; set; }

        public Byte TotalVideoChannels { get; set; }

        public AVChannel[] Channels { get; set; }
    }
}