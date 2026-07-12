using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>颜色信息 Box。描述媒体的颜色属性</summary>
    public class ColourInformationBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public ColourInformationBox() : base("colr") { }

        public String ColourType { get; set; }
        public UInt16 ColourPrimaries { get; set; }
        public UInt16 TransferCharacteristics { get; set; }
        public UInt16 MatrixCoefficients { get; set; }
        public Boolean FullRangeFlag { get; set; }
        public Byte Reserved { get; set; }
#warning ICC_profile?????
        public Byte[] ICCProfile { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            writer.WriteASCII(ColourType ?? "nclx");
            if (ColourType == "nclx")
            {
                writer.WriteUInt16(ColourPrimaries);
                writer.WriteUInt16(TransferCharacteristics);
                writer.WriteUInt16(MatrixCoefficients);
                writer.WriteByte((Byte)((FullRangeFlag ? 1 : 0) << 7 | (Reserved & 0x7F)));
            }
            End(ref writer);
        }
    }
}