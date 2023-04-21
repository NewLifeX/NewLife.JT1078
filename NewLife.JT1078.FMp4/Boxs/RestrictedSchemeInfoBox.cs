namespace NewLife.JT1078.FMp4
{
    public class RestrictedSchemeInfoBox : Mp4Box
    {
        public RestrictedSchemeInfoBox(String fmt) : base("rinf") => OriginalFormatBox = new OriginalFormatBox(fmt);

        public OriginalFormatBox OriginalFormatBox { get; set; }

        public SchemeTypeBox SchemeTypeBox { get; set; }

        public SchemeInformationBox SchemeInformationBox { get; set; }
    }
}