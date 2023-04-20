using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>clap</summary>
    public class CleanApertureBox : Mp4Box, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// clap
        /// </summary>
        public CleanApertureBox() : base("clap")
        {
        }

        public UInt32 CleanApertureWidthN { get; set; }
        public UInt32 CleanApertureWidthD { get; set; }
        public UInt32 CleanApertureHeightN { get; set; }
        public UInt32 CleanApertureHeightD { get; set; }
        public UInt32 HorizOffN { get; set; }
        public UInt32 HorizOffD { get; set; }
        public UInt32 VertOffN { get; set; }
        public UInt32 VertOffD { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            writer.WriteUInt32(CleanApertureWidthN);
            writer.WriteUInt32(CleanApertureWidthD);
            writer.WriteUInt32(CleanApertureHeightN);
            writer.WriteUInt32(CleanApertureHeightD);
            writer.WriteUInt32(HorizOffN);
            writer.WriteUInt32(HorizOffD);
            writer.WriteUInt32(VertOffN);
            writer.WriteUInt32(VertOffD);
            End(ref writer);
        }
    }
}