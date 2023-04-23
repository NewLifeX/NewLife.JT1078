using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>tkhd</summary>
    public class TrackHeaderBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>tkhd</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public TrackHeaderBox(Byte version, UInt32 flags) : base("tkhd", version, flags) { }

        /// <summary>tkhd</summary>
        /// <param name="version"></param>
        public TrackHeaderBox(Byte version) : base("tkhd", version, (FMp4Constants.TKHD_FLAG_ENABLED | FMp4Constants.TKHD_FLAG_IN_MOVIE | FMp4Constants.TKHD_FLAG_IN_PREVIEW)) { }

        public UInt64 CreationTime { get; set; }
        public UInt64 ModificationTime { get; set; }
        public UInt32 TrackID { get; set; }
        public UInt32 Reserved1 { get; set; }
        public UInt64 Duration { get; set; }
        public UInt32[] Reserved2 { get; set; } = new UInt32[2];
        public Int16 Layer { get; set; }
        public Int16 AlternateGroup { get; set; }
        public Boolean TrackIsAudio { get; set; } = false;
        public UInt16 Reserved3 { get; set; }
        public Int32[] Matrix { get; set; } = new Int32[9] { 0x00010000, 0, 0, 0, 0x00010000, 0, 0, 0, 0x40000000 };
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (Version == 1)
            {
                writer.WriteUInt64(CreationTime);
                writer.WriteUInt64(ModificationTime);
                writer.WriteUInt32(TrackID);
                writer.WriteUInt32(Reserved1);
                writer.WriteUInt64(Duration);
            }
            else
            {
                writer.WriteUInt32((UInt32)CreationTime);
                writer.WriteUInt32((UInt32)ModificationTime);
                writer.WriteUInt32(TrackID);
                writer.WriteUInt32(Reserved1);
                writer.WriteUInt32((UInt32)Duration);
            }
            foreach (var r in Reserved2)
            {
                writer.WriteUInt32(r);
            }
            writer.WriteInt16(Layer);
            writer.WriteInt16(AlternateGroup);
            if (TrackIsAudio)
            {
                writer.WriteInt16(0x0100);
            }
            else
            {
                writer.WriteInt16(0);
            }
            writer.WriteUInt16(Reserved3);
            foreach (var m in Matrix)
            {
                writer.WriteInt32(m);
            }
            //好奇葩的解析方式
            //实际
            //01 60 00 00
            //01 20 00 00
            //32位大端
            //00 00 01 60 
            //00 00 01 20
            //32位小端
            //60 01 00 00
            //20 01 00 00
            //直接用32位去解析是解析不出来的。
            writer.WriteUInt16((UInt16)Width);
            writer.WriteUInt16(0);
            writer.WriteUInt16((UInt16)Height);
            writer.WriteUInt16(0);
            End(ref writer);
        }
    }
}