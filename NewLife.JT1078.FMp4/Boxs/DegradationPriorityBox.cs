namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public class DegradationPriorityBox : FullBox
    {
        public DegradationPriorityBox(Byte version = 0, UInt32 flags = 0) : base("stdp", version, flags) { }

        /// <summary>
        /// sample_count is taken from the sample_count in the Sample Size Box ('stsz').
        /// </summary>
        public List<UInt16> Priorities { get; set; }
    }
}