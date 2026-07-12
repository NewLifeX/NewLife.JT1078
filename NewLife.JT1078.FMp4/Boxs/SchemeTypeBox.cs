using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>方案类型 Box。标识保护方案的类型和版本</summary>
    public class SchemeTypeBox : FullBox, IFMp4MessagePackFormatter
    {
        public SchemeTypeBox(Byte version, UInt32 flags) : base("schm", version, flags) { }

        public SchemeTypeBox(UInt32 flags) : this(0, flags) { }

        public UInt32 SchemeType { get; set; }

        public UInt32 SchemeVersion { get; set; }

        /// <summary>if (flags & 0x000001) UTF-8</summary>
        public String SchemeUri { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(SchemeType);
            writer.WriteUInt32(SchemeVersion);
            if ((Flags & 0x000001) != 0 && !String.IsNullOrEmpty(SchemeUri))
                writer.WriteUTF8(SchemeUri);
            End(ref writer);
        }
    }
}