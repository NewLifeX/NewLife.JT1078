using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>mvhd</summary>
    public class MovieHeaderBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>mvhd</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public MovieHeaderBox(Byte version, UInt32 flags = 0) : base("mvhd", version, flags) { }

        public UInt64 CreationTime { get; set; }

        public UInt64 ModificationTime { get; set; }

        public UInt32 Timescale { get; set; }

        public UInt64 Duration { get; set; }

        public Int32 Rate { get; set; } = 0x00010000;

        public Int16 Volume { get; set; } = 0x0100;

        public Byte[] Reserved1 { get; set; } = new Byte[2];

        public UInt32[] Reserved2 { get; set; } = new UInt32[2];

        public Int32[] Matrix { get; set; } = new Int32[9] { 0x00010000, 0, 0, 0, 0x00010000, 0, 0, 0, 0x40000000 };

        public Byte[] PreDefined { get; set; } = new Byte[24];

        public UInt32 NextTrackID { get; set; } = UInt32.MaxValue;

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
            writer.WriteInt32(Rate);
            writer.WriteInt16(Volume);
            foreach (var r in Reserved1)
            {
                writer.WriteByte(r);
            }
            foreach (var r in Reserved2)
            {
                writer.WriteUInt32(r);
            }
            foreach (var m in Matrix)
            {
                writer.WriteInt32(m);
            }
            foreach (var pd in PreDefined)
            {
                writer.WriteByte(pd);
            }
            writer.WriteUInt32(NextTrackID);
            End(ref writer);
        }
    }
}