namespace NewLife.JT1078.FMp4;

public class FilePartitionBox : FullBox
{
    public FilePartitionBox(Byte version = 0, UInt32 flags = 0) : base("fpar", version, flags) { }

    public UInt16 ItemID { get; set; }

    public UInt16 PacketPayloadSize { get; set; }

    public Byte Reserved { get; set; }

    public Byte FECEncodingID { get; set; }

    public UInt16 FECInstanceID { get; set; }

    public UInt16 MaxSourceBlockLength { get; set; }

    public UInt16 EncodingSymbolLength { get; set; }

    public UInt16 MaxNumberOfEncodingSymbols { get; set; }

    /// <summary>以null结尾</summary>
    public String SchemeSpecificInfo { get; set; }

    public UInt16 EntryCount { get; set; }

    public List<FilePartitionInfo> FilePartitionInfos { get; set; }

    public class FilePartitionInfo
    {
        public UInt16 BlockCount { get; set; }
        public UInt16 BlockSize { get; set; }
    }
}
