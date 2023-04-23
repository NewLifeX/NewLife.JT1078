namespace NewLife.JT1078.FMp4
{
    public class TrackReferenceTypeBox : Mp4Box
    {
        public TrackReferenceTypeBox(String referenceType) : base(referenceType) { }

        public List<UInt32> TrackIDs { get; set; }
    }
}