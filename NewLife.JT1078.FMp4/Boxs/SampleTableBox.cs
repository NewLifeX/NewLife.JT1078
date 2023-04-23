using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>stbl</summary>
    public class SampleTableBox : Mp4Box, IFMp4MessagePackFormatter
    {
        /// <summary>stbl</summary>
        public SampleTableBox() : base("stbl") { }

        /// <summary>stsd</summary>
        public SampleDescriptionBox SampleDescriptionBox { get; set; }

        /// <summary>stts</summary>
        public TimeToSampleBox TimeToSampleBox { get; set; }

        /// <summary>stss</summary>
        public SyncSampleBox SyncSampleBox { get; set; }

        /// <summary>ctts</summary>
        public CompositionOffsetBox CompositionOffsetBox { get; set; }

        /// <summary>stsz</summary>
        public SampleSizeBox SampleSizeBox { get; set; }

        /// <summary>stsc</summary>
        public SampleToChunkBox SampleToChunkBox { get; set; }

        /// <summary>stco</summary>
        public ChunkOffsetBox ChunkOffsetBox { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);

            SampleDescriptionBox.ToBuffer(ref writer);
            TimeToSampleBox.ToBuffer(ref writer);
            SampleToChunkBox.ToBuffer(ref writer);
            ChunkOffsetBox.ToBuffer(ref writer);

            SyncSampleBox?.ToBuffer(ref writer);
            CompositionOffsetBox?.ToBuffer(ref writer);
            SampleSizeBox?.ToBuffer(ref writer);

            End(ref writer);
        }
    }
}
