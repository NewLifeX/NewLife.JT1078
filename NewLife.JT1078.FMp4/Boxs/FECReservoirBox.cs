using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>FEC 储备 Box。包含前向纠错编码的储备信息</summary>
    public class FECReservoirBox : FullBox, IFMp4MessagePackFormatter
    {
        public FECReservoirBox(Byte version = 0, UInt32 flags = 0) : base("fecr", version, flags) { }

        public UInt16 EntryCount { get; set; }
        public List<FECReservoirInfo> FECReservoirInfos { get; set; }

        public class FECReservoirInfo
        {
            public UInt16 ItemID { get; set; }
            public UInt32 SymbolCount { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (FECReservoirInfos != null && FECReservoirInfos.Count > 0)
            {
                EntryCount = (UInt16)FECReservoirInfos.Count;
                writer.WriteUInt16(EntryCount);
                foreach (var info in FECReservoirInfos)
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
