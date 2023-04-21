namespace NewLife.JT1078.FMp4
{
    public class NullMediaHeaderBox : FullBox
    {
        public NullMediaHeaderBox(Byte version = 0, UInt32 flags = 0) : base("nmhd", version, flags) { }
    }
}
