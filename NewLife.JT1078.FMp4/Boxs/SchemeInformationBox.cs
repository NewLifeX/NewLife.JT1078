namespace NewLife.JT1078.FMp4
{
    public class SchemeInformationBox : Mp4Box
    {
        public SchemeInformationBox() : base("schi") { }

        public List<Mp4Box> SchemeSpecificData { get; set; }
    }
}