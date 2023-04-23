using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;
using NewLife.JT1078.FMp4.Samples;

namespace NewLife.JT1078.FMp4
{
    /// <summary>stsd</summary>
    public class SampleDescriptionBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>stsd</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SampleDescriptionBox(Byte version = 0, UInt32 flags = 0) : base("stsd", version, flags) { }

        private UInt32 EntryCount { get; set; }

        public List<SampleEntry> SampleEntries { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (SampleEntries != null && SampleEntries.Count > 0)
            {
                writer.WriteUInt32((UInt32)SampleEntries.Count);
                foreach (var item in SampleEntries)
                {
                    item.ToBuffer(ref writer);
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
