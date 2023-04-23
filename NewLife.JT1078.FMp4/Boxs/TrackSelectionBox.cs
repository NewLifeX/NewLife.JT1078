namespace NewLife.JT1078.FMp4
{
    public class TrackSelectionBox : FullBox
    {
        public TrackSelectionBox(Byte version = 0, UInt32 flags = 0) : base("tsel", version, flags) { }

        public UInt32 SwitchGroup { get; set; }

        public List<UInt32> AttributeList { get; set; }
    }
}