using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>填充位 Box。记录每个样本的填充位信息</summary>
    public class PaddingBitsBox : FullBox, IFMp4MessagePackFormatter
    {
        public PaddingBitsBox(Byte version = 0, UInt32 flags = 0) : base("padb", version, flags) { }

        public UInt32 SampleCount { get; set; }
        public List<PaddingBitsInfo> PaddingBitsInfos { get; set; }

        public class PaddingBitsInfo
        {
            public Boolean Reserved1 { get; set; }
            public Byte Pad1 { get; set; }
            public Boolean Reserved2 { get; set; }
            public Byte Pad2 { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (PaddingBitsInfos != null && PaddingBitsInfos.Count > 0)
            {
                SampleCount = (UInt32)PaddingBitsInfos.Count;
                writer.WriteUInt32(SampleCount);
                foreach (var info in PaddingBitsInfos)
                {
                    var b1 = (Byte)((info.Reserved1 ? 1 : 0) << 7 | (info.Pad1 & 0x7F));
                    var b2 = (Byte)((info.Reserved2 ? 1 : 0) << 7 | (info.Pad2 & 0x7F));
                    writer.WriteByte(b1);
                    writer.WriteByte(b2);
                }
            }
            else writer.WriteUInt32(0);
            End(ref writer);
        }
    }
}
