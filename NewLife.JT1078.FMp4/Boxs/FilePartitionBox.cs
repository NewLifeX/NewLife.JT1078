using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>文件分区 Box。描述文件的分区信息</summary>
    public class FilePartitionBox : FullBox, IFMp4MessagePackFormatter
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
        public String SchemeSpecificInfo { get; set; }
        public UInt16 EntryCount { get; set; }
        public List<FilePartitionInfo> FilePartitionInfos { get; set; }

        public class FilePartitionInfo
        {
            public UInt16 BlockCount { get; set; }
            public UInt16 BlockSize { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(ItemID);
            writer.WriteUInt16(PacketPayloadSize);
            writer.WriteByte(Reserved);
            writer.WriteByte(FECEncodingID);
            writer.WriteUInt16(FECInstanceID);
            writer.WriteUInt16(MaxSourceBlockLength);
            writer.WriteUInt16(EncodingSymbolLength);
            writer.WriteUInt16(MaxNumberOfEncodingSymbols);
            if (!String.IsNullOrEmpty(SchemeSpecificInfo)) writer.WriteUTF8(SchemeSpecificInfo);
            writer.WriteByte(0);
            if (FilePartitionInfos != null && FilePartitionInfos.Count > 0)
            {
                EntryCount = (UInt16)FilePartitionInfos.Count;
                writer.WriteUInt16(EntryCount);
                foreach (var info in FilePartitionInfos)
                {
                    writer.WriteUInt16(info.BlockCount);
                    writer.WriteUInt16(info.BlockSize);
                }
            }
            else writer.WriteUInt16(0);
            End(ref writer);
        }
    }
}
