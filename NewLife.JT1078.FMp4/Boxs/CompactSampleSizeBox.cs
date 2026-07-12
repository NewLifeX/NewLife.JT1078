using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>紧凑样本大小 Box。使用 4/8/16 位存储样本大小</summary>
    public class CompactSampleSizeBox : FullBox, IFMp4MessagePackFormatter
    {
        public CompactSampleSizeBox(Byte version = 0, UInt32 flags = 0) : base("stz2", version, flags) { }

        public Byte[] Reserved { get; set; } = new Byte[3];

        /// <summary>字段大小：4、8 或 16 位</summary>
        public Byte FieldSize { get; set; }

        /// <summary>样本数量</summary>
        public UInt32 SampleCount { get; set; }

        /// <summary>length:SampleCount 每个样本的大小</summary>
        public List<UInt16> EntrySize { get; set; }

        /// <summary>写入序列化数据</summary>
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteArray(Reserved);
            writer.WriteByte(FieldSize);
            if (EntrySize != null && EntrySize.Count > 0)
            {
                SampleCount = (UInt32)EntrySize.Count;
                writer.WriteUInt32(SampleCount);
                foreach (var item in EntrySize) writer.WriteUInt16(item);
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }
    }
}
