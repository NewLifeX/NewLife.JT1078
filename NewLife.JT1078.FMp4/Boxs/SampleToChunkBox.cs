using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>stsc</summary>
    public class SampleToChunkBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>stsc</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SampleToChunkBox(Byte version = 0, UInt32 flags = 0) : base("stsc", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public List<SampleToChunkInfo> SampleToChunkInfos { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (SampleToChunkInfos != null && SampleToChunkInfos.Count > 0)
            {
                writer.WriteUInt32((UInt32)SampleToChunkInfos.Count);
                foreach (var item in SampleToChunkInfos)
                {
                    writer.WriteUInt32(item.FirstChunk);
                    writer.WriteUInt32(item.SamplesPerChunk);
                    writer.WriteUInt32(item.SampleDescriptionIindex);
                }
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }

        public class SampleToChunkInfo
        {
            public UInt32 FirstChunk { get; set; }
            public UInt32 SamplesPerChunk { get; set; }
            public UInt32 SampleDescriptionIindex { get; set; }
        }
    }
}