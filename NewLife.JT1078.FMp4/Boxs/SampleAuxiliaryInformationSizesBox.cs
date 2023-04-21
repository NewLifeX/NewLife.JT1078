namespace NewLife.JT1078.FMp4
{

    public class SampleAuxiliaryInformationSizesBox : FullBox
    {
        public SampleAuxiliaryInformationSizesBox(Byte version = 0, UInt32 flags = 0) : base("saiz", version, flags) { }

        /// <summary>
        /// if (flags & 1)
        /// </summary>
        public UInt32 AuxInfoType { get; set; }

        /// <summary>
        /// if (flags & 1)
        /// </summary>
        public UInt32 AuxInfoTypeParameter { get; set; }

        public Byte DefaultSampleInfoSize { get; set; }

        public UInt32 SampleCount { get; set; }

        /// <summary>
        /// default_sample_info_size==0
        ///  length:sample_count
        /// </summary>
        public Byte[] SampleInfoSize { get; set; }
    }
}
