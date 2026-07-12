using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>立体视频 Box。描述立体视频配置和指示类型</summary>
    public class StereoVideoBox : FullBox, IFMp4MessagePackFormatter
    {
        public StereoVideoBox(Byte version = 0, UInt32 flags = 0) : base("stvi", version, flags) { }

        public UInt32 Reserved { get; set; }
        public Byte SingleViewAllowed { get; set; }
        public UInt32 StereoScheme { get; set; }
        public UInt32 Length { get; set; }
        /// <summary>length:Length</summary>
        public Byte[] StereoIndicationType { get; set; }
        public List<Mp4Box> AnyBox { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt24(Reserved);
            writer.WriteByte(SingleViewAllowed);
            writer.WriteUInt32(StereoScheme);
            writer.WriteUInt32(Length);
            if (StereoIndicationType != null) writer.WriteArray(StereoIndicationType);
            End(ref writer);
        }
    }
}