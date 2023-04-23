using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>vmhd</summary>
    public class VideoMediaHeaderBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>vmhd</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public VideoMediaHeaderBox(Byte version = 0, UInt32 flags = 1) : base("vmhd", version, flags) { }

        public UInt16 GraphicsMode { get; set; }
        public UInt16 Red { get; set; }
        public UInt16 Green { get; set; }
        public UInt16 Blue { get; set; }
        public UInt16[] OpColor => new UInt16[] { Red, Green, Blue };

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(GraphicsMode);

            foreach (var item in OpColor)
            {
                writer.WriteUInt16(item);
            }

            End(ref writer);
        }
    }
}