namespace NewLife.JT1078.FMp4
{
    public class ProtectionSchemeInfoBox : Mp4Box
    {
        public ProtectionSchemeInfoBox(String fmt) : base("sinf") => OriginalFormatBox = new OriginalFormatBox(fmt);

        public OriginalFormatBox OriginalFormatBox { get; set; }

        public SchemeTypeBox SchemeTypeBox { get; set; }

        public SchemeInformationBox SchemeInformationBox { get; set; }
    }
}