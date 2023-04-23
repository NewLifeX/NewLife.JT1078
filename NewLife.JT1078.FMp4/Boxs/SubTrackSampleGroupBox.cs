namespace NewLife.JT1078.FMp4
{
    public class SubTrackSampleGroupBox : FullBox
    {
        public SubTrackSampleGroupBox(Byte version = 0, UInt32 flags = 0) : base("stsg", version, flags) { }

        public UInt32 GroupingType { get; set; }

        public UInt16 ItemCount { get; set; }

        /// <summary>length:ItemCount</summary>
        public List<UInt32> GroupDescriptionIndex { get; set; }
    }
}