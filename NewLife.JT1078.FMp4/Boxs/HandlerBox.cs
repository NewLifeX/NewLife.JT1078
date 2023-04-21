using NewLife.JT1078.FMp4.Enums;
using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>hdlr</summary>
    public class HandlerBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>hdlr</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public HandlerBox(Byte version = 0, UInt32 flags = 0) : base("hdlr", version, flags) { }

        public UInt32 PreDefined { get; set; }

        public HandlerType HandlerType { get; set; }

        public UInt32[] Reserved { get; set; } = new UInt32[3];

        public String Name { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(PreDefined);
            if (HandlerType == HandlerType.none)
            {
                writer.WriteASCII("null");
            }
            else
            {
                writer.WriteASCII(HandlerType.ToString());
            }
            foreach (var r in Reserved)
            {
                writer.WriteUInt32(r);
            }
            if (!String.IsNullOrEmpty(Name))
            {
                writer.WriteUTF8(Name);
                writer.WriteUTF8("\0");
            }
            else
            {
                writer.WriteUTF8("\0");
            }
            End(ref writer);
        }
    }
}
