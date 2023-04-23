using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>uri</summary>
    public class URIBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>uri</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public URIBox(Byte version = 0, UInt32 flags = 0) : base("uri ", version, flags) { }

        public String TheURI { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);

            WriterFullBoxToBuffer(ref writer);
            writer.WriteUTF8(TheURI ?? "\0");

            End(ref writer);
        }
    }
}