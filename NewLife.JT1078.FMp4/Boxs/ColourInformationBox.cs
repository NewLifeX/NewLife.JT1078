namespace NewLife.JT1078.FMp4
{
    public class ColourInformationBox : Mp4Box
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
    }
}