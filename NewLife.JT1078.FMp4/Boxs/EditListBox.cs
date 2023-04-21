namespace NewLife.JT1078.FMp4
{
    public class EditListBox : FullBox
    {
        public EditListBox(Byte version, UInt32 flags = 0) : base("elst", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public UInt64 SegmentDurationLarge { get; set; }

        public Int64 MediaTimeLarge { get; set; }

        public UInt32 SegmentDuration { get; set; }

        public Int32 MediaTime { get; set; }

        public Int16 MediaRateInteger { get; set; }

        public Int16 MediaRateFraction { get; set; }
    }
}