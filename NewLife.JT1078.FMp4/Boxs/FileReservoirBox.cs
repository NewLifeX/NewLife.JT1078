namespace NewLife.JT1078.FMp4;

public class FileReservoirBox : FullBox
{
    public FileReservoirBox(Byte version = 0, UInt32 flags = 0) : base("fire", version, flags) { }

    public UInt16 EntryCount { get; set; }

    public List<FileReservoirInfo> FileReservoirInfos { get; set; }

    public class FileReservoirInfo
    {
        public UInt16 ItemID { get; set; }
        public UInt32 SymbolCount { get; set; }
    }
}
