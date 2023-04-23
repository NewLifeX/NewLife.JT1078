using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary>avc1</summary>
    public class AVC1SampleEntry : VisualSampleEntry
    {
        /// <summary>avc1</summary>
        public AVC1SampleEntry() : base("avc1")
        {
            DataReferenceIndex = 1;
            PreDefined3 = 0xffff;
        }

        /// <summary>avcC</summary>
        public AVCConfigurationBox AVCConfigurationBox { get; set; }

        /// <summary>btrt</summary>
        public MPEG4BitRateBox MPEG4BitRateBox { get; set; }

        public override void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);

            WriterSampleEntryToBuffer(ref writer);
            WriterVisualSampleEntryToBuffer(ref writer);

            AVCConfigurationBox?.ToBuffer(ref writer);
            MPEG4BitRateBox?.ToBuffer(ref writer);

            End(ref writer);
        }
    }
}