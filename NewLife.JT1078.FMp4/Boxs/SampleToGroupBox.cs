using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// sbgp
    /// </summary>
    public class SampleToGroupBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// sbgp
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SampleToGroupBox(Byte version, UInt32 flags) : base("sbgp", version, flags) { }

        public UInt32 GroupingType { get; set; }

        /// <summary>
        /// version == 1
        /// </summary>
        public UInt32 GroupingTypeParameter { get; set; }

        public UInt32 EntryCount { get; set; }

        public List<SampleToGroupInfo> SampleToGroupInfos { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            writer.WriteUInt32(GroupingType);
            if (Version == 1)
            {
                writer.WriteUInt32(GroupingTypeParameter);
            }
            if (SampleToGroupInfos != null && SampleToGroupInfos.Count > 0)
            {
                writer.WriteUInt32((UInt32)SampleToGroupInfos.Count);
                foreach (var item in SampleToGroupInfos)
                {
                    writer.WriteUInt32(item.SampleCount);
                    writer.WriteUInt32(item.GroupDescriptionIndex);
                }
            }
            else
            {
                writer.WriteUInt32(0);
            }
        }

        public class SampleToGroupInfo
        {
            public UInt32 SampleCount { get; set; }
            public UInt32 GroupDescriptionIndex { get; set; }
        }
    }
}