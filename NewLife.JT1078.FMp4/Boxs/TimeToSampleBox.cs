using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// stts
    /// </summary>
    public class TimeToSampleBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// stts
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public TimeToSampleBox(Byte version = 0, UInt32 flags = 0) : base("stts", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public List<TimeToSampleInfo> TimeToSampleInfos { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (TimeToSampleInfos != null && TimeToSampleInfos.Count > 0)
            {
                writer.WriteUInt32((UInt32)TimeToSampleInfos.Count);
                foreach (var item in TimeToSampleInfos)
                {
                    writer.WriteUInt32(item.SampleCount);
                    writer.WriteUInt32(item.SampleDelta);
                }
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }

        public class TimeToSampleInfo
        {
            public UInt32 SampleCount { get; set; }

            public UInt32 SampleDelta { get; set; }
        }
    }
}