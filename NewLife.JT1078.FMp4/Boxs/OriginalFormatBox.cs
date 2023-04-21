namespace NewLife.JT1078.FMp4
{
    public class OriginalFormatBox : Mp4Box
    {
        public OriginalFormatBox(String codingname) : base("frma") => DataFormat = codingname;

        public String DataFormat { get; set; }
    }
}