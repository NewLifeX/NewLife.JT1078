using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>提示媒体头 Box。包含提示轨道的 PDU 大小和比特率信息</summary>
    public class HintMediaHeaderBox : FullBox, IFMp4MessagePackFormatter
    {
        public HintMediaHeaderBox(Byte version = 0, UInt32 flags = 0) : base("hmhd", version, flags) { }

        public UInt16 MaxPDUSize { get; set; }
        public UInt16 AvgPDUSize { get; set; }
        public UInt16 MaxBitRate { get; set; }
        public UInt16 AvgBitRate { get; set; }
        public UInt16 Reserved { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(MaxPDUSize);
            writer.WriteUInt16(AvgPDUSize);
            writer.WriteUInt16(MaxBitRate);
            writer.WriteUInt16(AvgBitRate);
            writer.WriteUInt16(Reserved);
            End(ref writer);
        }
    }
}