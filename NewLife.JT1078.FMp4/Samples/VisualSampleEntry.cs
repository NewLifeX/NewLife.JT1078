using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Samples
{
    /// <summary>VisualSampleEntry</summary>
    public abstract class VisualSampleEntry : SampleEntry
    {
        /// <summary>VisualSampleEntry</summary>
        /// <param name="boxType"></param>
        public VisualSampleEntry(String boxType) : base(boxType) { }

        public UInt16 PreDefined1 { get; set; }

        public UInt16 Reserved1 { get; set; }

        public UInt32[] PreDefined2 { get; set; } = new UInt32[3];

        public UInt16 Width { get; set; }

        public UInt16 Height { get; set; }

        public UInt32 HorizreSolution { get; set; } = 0x00480000;

        public UInt32 VertreSolution { get; set; } = 0x00480000;

        public UInt32 Reserved3 { get; set; }

        public UInt16 FrameCount { get; set; } = 1;
#if DEBUG
        public String CompressorName { get; set; }
#else
    public string CompressorName { get; set; } = "NewLife.JT1078";
#endif
        public UInt16 Depth { get; set; } = 0x0018;

        public UInt16 PreDefined3 { get; set; } = 0x1111;

        /// <summary>
        /// clap
        /// optional
        /// </summary>
        public CleanApertureBox Clap { get; set; }

        /// <summary>
        /// pasp
        /// optional
        /// </summary>
        public PixelAspectRatioBox Pasp { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected void WriterVisualSampleEntryToBuffer(ref FMp4MessagePackWriter writer)
        {
            writer.WriteUInt16(PreDefined1);
            writer.WriteUInt16(Reserved1);
            foreach (var item in PreDefined2)
            {
                writer.WriteUInt32(item);
            }
            writer.WriteUInt16(Width);
            writer.WriteUInt16(Height);
            writer.WriteUInt32(HorizreSolution);
            writer.WriteUInt32(VertreSolution);
            writer.WriteUInt32(Reserved3);
            writer.WriteUInt16(FrameCount);
            if (String.IsNullOrEmpty(CompressorName))
            {
                //CompressorNameLength
                writer.WriteByte(0);
                writer.WriteArray(new Byte[31]);
            }
            else
            {
                //CompressorNameLength
                writer.WriteByte((Byte)CompressorName.Length);
                writer.WriteUTF8(CompressorName.PadRight(31, '\0'));
            }
            writer.WriteUInt16(Depth);
            writer.WriteUInt16(PreDefined3);
        }
    }
}