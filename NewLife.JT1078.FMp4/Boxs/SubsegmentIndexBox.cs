using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>子分段索引 Box。定义子分段的范围信息</summary>
    public class SubsegmentIndexBox : FullBox, IFMp4MessagePackFormatter
    {
        public SubsegmentIndexBox(Byte version = 0, UInt32 flags = 0) : base("ssix", version, flags) { }

        public UInt32 SubSegmentCount { get; set; }

        /// <summary>length:SubSegmentCount</summary>
        public List<SubsegmentIndexInfo> SubsegmentIndexInfos { get; set; }

        public class SubsegmentIndexInfo
        {
            /// <summary></summary>
            public UInt32 RangesCount { get; set; }

            /// <summary>length:RangesCount</summary>
            public List<SubsegmentRangeInfo> SubsegmentRangeInfos { get; set; } = new List<SubsegmentRangeInfo>();

            public class SubsegmentRangeInfo
            {
                /// <summary>32 - 8</summary>
                public Byte Level { get; set; }

                /// <summary>32 - 4</summary>
                public UInt32 RangeSize { get; set; }
            }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (SubsegmentIndexInfos != null && SubsegmentIndexInfos.Count > 0)
            {
                SubSegmentCount = (UInt32)SubsegmentIndexInfos.Count;
                writer.WriteUInt32(SubSegmentCount);
                foreach (var info in SubsegmentIndexInfos)
                {
                    if (info.SubsegmentRangeInfos != null)
                    {
                        info.RangesCount = (UInt32)info.SubsegmentRangeInfos.Count;
                        writer.WriteUInt32(info.RangesCount);
                        foreach (var range in info.SubsegmentRangeInfos)
                        {
                            writer.WriteUInt24(range.Level);
                            writer.WriteUInt32(range.RangeSize);
                        }
                    }
                }
            }
            else writer.WriteUInt32(0);
            End(ref writer);
        }
    }
}