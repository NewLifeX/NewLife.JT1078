namespace NewLife.JT1078.FMp4;

public class PrimaryItemBox : FullBox
{
    public PrimaryItemBox(Byte version = 0, UInt32 flags = 0) : base("pitm", version, flags) { }

    public UInt16 ItemID { get; set; }
}
