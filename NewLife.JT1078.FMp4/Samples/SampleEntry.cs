using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary>SampleEntry</summary>
    public abstract class SampleEntry : Mp4Box, IFMp4MessagePackFormatter
    {
        /// <summary>SampleEntry</summary>
        /// <param name="boxType"></param>
        public SampleEntry(String boxType) : base(boxType) { }

        public Byte[] Reserved { get; set; } = new Byte[6];

        public UInt16 DataReferenceIndex { get; set; }

        /// <summary></summary>
        /// <param name="writer"></param>
        protected void WriterSampleEntryToBuffer(ref FMp4MessagePackWriter writer)
        {
            foreach (var item in Reserved)
            {
                writer.WriteByte(item);
            }
            writer.WriteUInt16(DataReferenceIndex);
        }

        public abstract void ToBuffer(ref FMp4MessagePackWriter writer);
    }
}