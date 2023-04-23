namespace NewLife.JT1078.FMp4
{
    public class SoundMediaHeaderBox : FullBox
    {
        public SoundMediaHeaderBox(Byte version = 0, UInt32 flags = 0) : base("smhd", version, flags) { }

        public UInt16 Balance { get; set; }

        public UInt16 Reserved { get; set; }
    }
}