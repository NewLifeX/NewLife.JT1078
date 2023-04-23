using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary>metx</summary>
    public class XMLMetaDataSampleEntry : SampleEntry
    {
        /// <summary>metx</summary>
        public XMLMetaDataSampleEntry() : base("metx") { }

        /// <summary>optional</summary>
        public String ContentEncoding { get; set; }

        /// <summary>Namespace</summary>
        public String Namespace { get; set; }

        /// <summary>optional</summary>
        public String SchemaLocation { get; set; }

        /// <summary>optional</summary>
        public MPEG4BitRateBox BitRateBox { get; set; }

        public override void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterSampleEntryToBuffer(ref writer);
            if (!String.IsNullOrEmpty(ContentEncoding))
            {
                writer.WriteUTF8(ContentEncoding);
            }
            writer.WriteUTF8(Namespace ?? "\0");
            if (!String.IsNullOrEmpty(SchemaLocation))
            {
                writer.WriteUTF8(SchemaLocation);
            }
            BitRateBox?.ToBuffer(ref writer);

            End(ref writer);
        }
    }
}