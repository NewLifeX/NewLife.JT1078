using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary></summary>
    public abstract class AudioSampleEntry : SampleEntry
    {
        public AudioSampleEntry(String codingname) : base(codingname) { }

        public UInt32[] Reserved1 { get; set; } = new UInt32[2];

        public UInt16 ChannelCount { get; set; } = 2;

        public UInt16 SampleSize { get; set; } = 16;

        public UInt16 PreDefined { get; set; } = 0;

        public UInt16 Reserved2 { get; set; } = 0;

        /// <summary>
        ///  default samplerate of media << 16;
        /// </summary>
        public UInt32 SampleRate { get; set; }

        protected void WriterAudioSampleEntryToBuffer(ref FMp4MessagePackWriter writer)
        {
            foreach (var item in Reserved1)
            {
                writer.WriteUInt32(item);
            }

            writer.WriteUInt16(ChannelCount);
            writer.WriteUInt16(SampleSize);
            writer.WriteUInt16(PreDefined);
            writer.WriteUInt16(Reserved2);
            writer.WriteUInt32(SampleRate << 16);
        }
    }
}