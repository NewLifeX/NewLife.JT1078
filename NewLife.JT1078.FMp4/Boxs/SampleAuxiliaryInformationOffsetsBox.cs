namespace NewLife.JT1078.FMp4
{
    public class SampleAuxiliaryInformationOffsetsBox : FullBox
    {
        public SampleAuxiliaryInformationOffsetsBox(Byte version, UInt32 flags) : base("saio", version, flags) { }

        /// <summary>
        /// if (flags & 1)
        /// </summary>
        public UInt32 AuxInfoType { get; set; }

        /// <summary>
        /// if (flags & 1)
        /// </summary>
        public UInt32 AuxInfoTypeParameter { get; set; }

        public UInt32 EntryCount { get; set; }

        /// <summary>
        /// length:entry_count
        /// </summary>
        public UInt32[] Offset { get; set; }

        /// <summary>
        /// length:entry_count
        /// </summary>
        public UInt64[] OffsetLarge { get; set; }
    }
}