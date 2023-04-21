using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>ctts</summary>
    public class CompositionOffsetBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>ctts</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public CompositionOffsetBox(Byte version = 0, UInt32 flags = 0) : base("ctts", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public List<CompositionOffsetInfo> CompositionOffsetInfos { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (CompositionOffsetInfos != null && CompositionOffsetInfos.Count > 0)
            {
                writer.WriteUInt32((UInt32)CompositionOffsetInfos.Count);
                foreach (var item in CompositionOffsetInfos)
                {
                    if (Version == 0)
                    {
                        writer.WriteUInt32(item.SampleCount);
                        writer.WriteUInt32(item.SampleOffset);
                    }
                    else
                    {
                        writer.WriteUInt32(item.SampleCount);
                        writer.WriteInt32(item.SignedSampleOffset);
                    }
                }
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }

        public class CompositionOffsetInfo
        {
            public UInt32 SampleCount { get; set; }

            public UInt32 SampleOffset { get; set; }

            /// <summary>
            /// version == 1
            /// </summary>
            public Int32 SignedSampleOffset { get; set; }
        }
    }
}