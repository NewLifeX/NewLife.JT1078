using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>版权信息 Box。包含版权声明字符串</summary>
    public class CopyrightBox : FullBox, IFMp4MessagePackFormatter
    {
        public CopyrightBox(Byte version = 0, UInt32 flags = 0) : base("cprt", version, flags) { }

        /// <summary>16-1 填充位</summary>
        public Boolean Pad { get; set; }

        /// <summary>16-15 ISO-639-2/T 语言代码</summary>
        public Byte Language { get; set; }

        /// <summary>版权声明字符串</summary>
        public String Notice { get; set; }

        /// <summary>写入序列化数据</summary>
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            UInt16 lang = (UInt16)((Pad ? 1 : 0) << 15 | Language);
            writer.WriteUInt16(lang);
            if (!String.IsNullOrEmpty(Notice)) writer.WriteUTF8(Notice);
            End(ref writer);
        }
    }
}
