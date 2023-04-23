using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>stsz</summary>
    public class SampleSizeBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>stsz</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SampleSizeBox(Byte version = 0, UInt32 flags = 0) : base("stsz", version, flags) { }

        public UInt32 SampleSize { get; set; }

        public UInt32 SampleCount { get; set; }

        /// <summary>
        ///  if (sample_size==0)
        ///  length:sample_count
        /// </summary>
        public List<UInt32> EntrySize { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(SampleSize);
            if (EntrySize != null && EntrySize.Count > 0)
            {
                writer.WriteUInt32((UInt32)EntrySize.Count);
                foreach (var item in EntrySize)
                {
                    writer.WriteUInt32(item);
                }
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }
    }
}