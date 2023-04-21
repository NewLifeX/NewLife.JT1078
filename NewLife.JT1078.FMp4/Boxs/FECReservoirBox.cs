namespace NewLife.JT1078.FMp4;

public class FECReservoirBox : FullBox
{
    public FECReservoirBox(Byte version = 0, UInt32 flags = 0) : base("fecr", version, flags) { }

    public UInt16 EntryCount { get; set; }

    public List<FECReservoirInfo> FECReservoirInfos { get; set; }

    public class FECReservoirInfo
    {
        public UInt16 ItemID { get; set; }
        public UInt32 SymbolCount { get; set; }
    }
}
