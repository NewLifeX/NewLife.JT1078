using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary>HintSampleEntry</summary>
    public class HintSampleEntry : SampleEntry
    {
        /// <summary>HintSampleEntry</summary>
        /// <param name="protocol"></param>
        public HintSampleEntry(String protocol) : base(protocol) { }

        public List<Byte> Data { get; set; } = new List<Byte>();

        public override void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterSampleEntryToBuffer(ref writer);
            if (Data != null && Data.Count > 0)
            {
                foreach (var item in Data)
                {
                    writer.WriteByte(item);
                }
            }
            End(ref writer);
        }
    }
}