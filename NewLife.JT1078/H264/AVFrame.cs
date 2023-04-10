namespace NewLife.JT1078.H264
{

    public class AVFrame
    {
        /// <summary>
        /// 终端设备SIM卡号
        /// BCD[6]
        /// </summary>
        public String SIM { get; set; }
        /// <summary>
        /// 逻辑通道号
        /// </summary>
        public Byte LogicChannelNumber { get; set; }
        public List<H264NALU> Nalus { get; set; }
        public H264NALU SPS { get; set; }
        public H264NALU PPS { get; set; }
        public Int32 Width { get; set; }
        public Int32 Height { get; set; }
        private String VideoType { get; set; } = "avc1";
        public Byte ProfileIdc { get; set; }
        public Byte ProfileCompat { get; set; }
        public Byte LevelIdc { get; set; }
        public String ToCodecs() => $"{VideoType}.{ProfileIdc:x2}{ProfileCompat:x2}{LevelIdc:x2}";
        public String GetAVKey() => $"{SIM}_{LogicChannelNumber}";
    }
}