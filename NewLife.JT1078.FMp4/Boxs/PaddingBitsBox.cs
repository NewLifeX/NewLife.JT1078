namespace NewLife.JT1078.FMp4
{
    public class PaddingBitsBox : FullBox
    {
        public PaddingBitsBox(Byte version = 0, UInt32 flags = 0) : base("padb", version, flags) { }

        public UInt32 SampleCount { get; set; }

        public List<PaddingBitsInfo> PaddingBitsInfos { get; set; }

        public class PaddingBitsInfo
        {
            public Boolean Reserved1 { get; set; }
            public Byte Pad1 { get; set; }
            public Boolean Reserved2 { get; set; }
            public Byte Pad2 { get; set; }
        }
    }
}
