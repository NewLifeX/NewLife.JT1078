namespace NewLife.JT1078.FMp4
{

    public class TrackGroupTypeBox : FullBox
    {
        public TrackGroupTypeBox(String boxType, Byte version = 0, UInt32 flags = 0) : base(boxType, version, flags) { }

        public UInt32 TrackGroupId { get; set; }
    }
}