namespace NewLife.JT1078.FMp4
{
    public class ItemProtectionBox : FullBox
    {
        public ItemProtectionBox(Byte version = 0, UInt32 flags = 0) : base("ipro", version, flags) { }

        public UInt16 ProtectionCount { get; set; }
    }
}
