using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>stss</summary>
    public class SyncSampleBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>stss</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SyncSampleBox(Byte version = 0, UInt32 flags = 0) : base("stss", version, flags) { }

        public List<UInt32> SampleNumbers { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);

            WriterFullBoxToBuffer(ref writer);
            if (SampleNumbers != null && SampleNumbers.Count > 0)
            {
                writer.WriteUInt32((UInt32)SampleNumbers.Count);
                foreach (var item in SampleNumbers)
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