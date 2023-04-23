using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>pasp</summary>
    public class PixelAspectRatioBox : Mp4Box, IFMp4MessagePackFormatter
    {
        /// <summary>pasp</summary>
        public PixelAspectRatioBox() : base("pasp") { }

        public UInt32 HSpacing { get; set; }

        public UInt32 VSpacing { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            writer.WriteUInt32(HSpacing);
            writer.WriteUInt32(VSpacing);
            End(ref writer);
        }
    }
}
