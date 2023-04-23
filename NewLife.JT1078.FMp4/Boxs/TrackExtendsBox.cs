using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// trex
    /// </summary>
    public class TrackExtendsBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// trex
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public TrackExtendsBox(Byte version = 0, UInt32 flags = 0) : base("trex", version, flags)
        {
        }

        public UInt32 TrackID { get; set; }
        public UInt32 DefaultSampleDescriptionIndex { get; set; }
        public UInt32 DefaultSampleDuration { get; set; }
        public UInt32 DefaultSampleSize { get; set; }
        public UInt32 DefaultSampleFlags { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(TrackID);
            writer.WriteUInt32(DefaultSampleDescriptionIndex);
            writer.WriteUInt32(DefaultSampleDuration);
            writer.WriteUInt32(DefaultSampleSize);
            writer.WriteUInt32(DefaultSampleFlags);
            End(ref writer);
        }
    }
}