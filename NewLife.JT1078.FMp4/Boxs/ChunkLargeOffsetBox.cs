using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>大文件块偏移表 Box。与 stco 相同但使用 64 位偏移量</summary>
    public class ChunkLargeOffsetBox : FullBox, IFMp4MessagePackFormatter
    {
        public ChunkLargeOffsetBox(Byte version = 0, UInt32 flags = 0) : base("co64", version, flags) { }

        public UInt32 EntryCount { get; set; }

        /// <summary>length:EntryCount</summary>
        public List<UInt64> ChunkOffset { get; set; }

        /// <summary>写入序列化数据</summary>
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (ChunkOffset != null && ChunkOffset.Count > 0)
            {
                EntryCount = (UInt32)ChunkOffset.Count;
                writer.WriteUInt32(EntryCount);
                foreach (var item in ChunkOffset) writer.WriteUInt64(item);
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }
    }
}