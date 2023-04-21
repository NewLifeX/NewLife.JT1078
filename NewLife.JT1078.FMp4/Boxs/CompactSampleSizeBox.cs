namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public class CompactSampleSizeBox : FullBox
    {
        public CompactSampleSizeBox(Byte version = 0, UInt32 flags = 0) : base("stz2", version, flags) { }

        public Byte[] Reserved { get; set; } = new Byte[3];

        /// <summary>
        /// 4, 8 or 16
        /// </summary>
        public Byte FieldSize { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UInt32 SampleCount { get; set; }

        /// <summary>
        /// length:SampleCount
        /// DepOn:field_size=>DataType
        /// </summary>
        public List<UInt16> EntrySize { get; set; }
    }
}
