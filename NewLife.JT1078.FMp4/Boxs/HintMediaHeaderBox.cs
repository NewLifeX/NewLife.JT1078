namespace NewLife.JT1078.FMp4
{
    public class HintMediaHeaderBox : FullBox
    {
        public HintMediaHeaderBox(Byte version = 0, UInt32 flags = 0) : base("hmhd", version, flags) { }

        public UInt16 MaxPDUSize { get; set; }
        public UInt16 AvgPDUSize { get; set; }
        public UInt16 MaxBitRate { get; set; }
        public UInt16 AvgBitRate { get; set; }
        public UInt16 Reserved { get; set; }
    }
}