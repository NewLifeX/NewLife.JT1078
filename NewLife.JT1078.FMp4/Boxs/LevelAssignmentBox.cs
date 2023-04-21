namespace NewLife.JT1078.FMp4
{
    public class LevelAssignmentBox : FullBox
    {
        public LevelAssignmentBox(Byte version = 0, UInt32 flags = 0) : base("leva", version, flags) { }

        public Byte LevelCount { get; set; }

        public class LevelAssignmentInfo
        {
            public UInt32 TrackId { get; set; }
           
            /// <summary>
            /// 1byte 8-1
            /// </summary>
            public Boolean PaddingFlag { get; set; }
            
            /// <summary>
            /// 1byte 8-7
            /// </summary>
            public Byte AssignmentType { get; set; }
            
            /// <summary>
            /// AssignmentType == 0 || assignment_type == 1
            /// </summary>
            public UInt32 GroupingType { get; set; }
            
            /// <summary>
            /// assignment_type == 1
            /// </summary>
            public UInt32 GroupingTypeParameter { get; set; }
            
            /// <summary>
            /// assignment_type == 4
            /// </summary>
            public UInt32 SubTrackId { get; set; }
        }
    }
}
