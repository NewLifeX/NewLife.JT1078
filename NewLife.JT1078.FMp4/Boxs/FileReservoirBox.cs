using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>文件储备 Box。包含文件储备信息</summary>
    public class FileReservoirBox : FullBox, IFMp4MessagePackFormatter
    {
        public FileReservoirBox(Byte version = 0, UInt32 flags = 0) : base("fire", version, flags) { }

        public UInt16 EntryCount { get; set; }
        public List<FileReservoirInfo> FileReservoirInfos { get; set; }

        public class FileReservoirInfo
        {
            public UInt16 ItemID { get; set; }
            public UInt32 SymbolCount { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (FileReservoirInfos != null && FileReservoirInfos.Count > 0)
            {
                EntryCount = (UInt16)FileReservoirInfos.Count;
                writer.WriteUInt16(EntryCount);
                foreach (var info in FileReservoirInfos)
                {
                    writer.WriteUInt16(info.ItemID);
                    writer.WriteUInt32(info.SymbolCount);
                }
            }
            else writer.WriteUInt16(0);
            End(ref writer);
        }
    }
}
