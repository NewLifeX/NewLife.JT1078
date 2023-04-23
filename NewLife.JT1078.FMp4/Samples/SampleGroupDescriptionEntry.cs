namespace NewLife.JT1078.FMp4.Samples
{
    public abstract class SampleGroupDescriptionEntry
    {
        public UInt32 GroupingType { get; set; }

        public SampleGroupDescriptionEntry(UInt32 groupingType) => GroupingType = groupingType;
    }
}