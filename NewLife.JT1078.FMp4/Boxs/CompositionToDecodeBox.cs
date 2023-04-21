namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public class CompositionToDecodeBox : FullBox
    {
        public CompositionToDecodeBox(Byte version = 0, UInt32 flags = 0) : base("cslg", version, flags) { }

        public Int32 CompositionToDTSShift { get; set; }

        public Int32 LeastDecodeToDisplayDelta { get; set; }

        public Int32 GreatestDecodeToDisplayDelta { get; set; }

        public Int32 CompositionStartTime { get; set; }

        public Int32 CompositionEndTime { get; set; }
    }
}