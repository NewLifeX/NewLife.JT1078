using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// mdhd
    /// </summary>
    public class MediaHeaderBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// mdhd
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public MediaHeaderBox(Byte version = 0, UInt32 flags = 0) : base("mdhd", version, flags)        {        }

        public UInt64 CreationTime { get; set; }
        public UInt64 ModificationTime { get; set; }
        public UInt32 Timescale { get; set; }
        public UInt64 Duration { get; set; } = 1;
        //public bool Pad { get; set; }

        /// <summary>
        /// ISO-639-2/T language code
        /// ref:doc\fmp4\ISO Language Codes.txt
        /// und-undetermined
        /// </summary>
        public String Language { get; set; } = "und";
        public UInt16 PreDefined { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (Version == 1)
            {
                writer.WriteUInt64(CreationTime);
                writer.WriteUInt64(ModificationTime);
                writer.WriteUInt32(Timescale);
                writer.WriteUInt64(Duration);
            }
            else
            {
                writer.WriteUInt32((UInt32)CreationTime);
                writer.WriteUInt32((UInt32)ModificationTime);
                writer.WriteUInt32(Timescale);
                writer.WriteUInt32((UInt32)Duration);
            }
            writer.WriteIso639(Language);
            writer.WriteUInt16(PreDefined);
            End(ref writer);
        }
    }
}
