namespace NewLife.JT1078.FMp4
{
    public class SubsegmentIndexBox : FullBox
    {
        public SubsegmentIndexBox(Byte version = 0, UInt32 flags = 0) : base("ssix", version, flags) { }

        public UInt32 SubSegmentCount { get; set; }

        /// <summary>
        /// length:SubSegmentCount
        /// </summary>
        public List<SubsegmentIndexInfo> SubsegmentIndexInfos { get; set; }

        public class SubsegmentIndexInfo
        {
            public UInt32 RangesCount { get; set; }

            /// <summary>
            /// length:RangesCount
            /// </summary>
            public List<SubsegmentRangeInfo> SubsegmentRangeInfos { get; set; }

            public class SubsegmentRangeInfo
            {
                /// <summary>
                /// 32 - 8
                /// </summary>
                public Byte Level { get; set; }

                /// <summary>
                ///  32 - 4
                /// </summary>
                public UInt32 RangeSize { get; set; }
            }
        }
    }
}