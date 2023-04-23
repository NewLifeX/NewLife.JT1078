using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary>mett</summary>
    public class TextMetaDataSampleEntry : SampleEntry
    {
        /// <summary>mett</summary>
        public TextMetaDataSampleEntry() : base("mett") { }

        /// <summary>optional</summary>
        public String ContentEncoding { get; set; }

        /// <summary></summary>
        public String MimeFormat { get; set; }

        /// <summary>
        /// btrt
        /// optional
        /// </summary>
        public MPEG4BitRateBox BitRateBox { get; set; }

        public override void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            if (!String.IsNullOrEmpty(ContentEncoding))
            {
                writer.WriteUTF8(ContentEncoding);
            }
            writer.WriteUTF8(MimeFormat ?? "\0");
            BitRateBox?.ToBuffer(ref writer);
        }
    }
}