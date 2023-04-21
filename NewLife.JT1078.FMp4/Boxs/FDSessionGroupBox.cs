namespace NewLife.JT1078.FMp4;

public class FDSessionGroupBox : Mp4Box
{
    public FDSessionGroupBox() : base("segr") { }

    public UInt16 NumSessionGroups { get; set; }

    public class FDSessionGroupInfo
    {
        public Byte EntryCount { get; set; }

        /// <summary>length:EntryCount</summary>
        public List<UInt32> GroupIDs { get; set; }

        /// <summary></summary>
        public UInt16 NumChannelsInSessionGroup { get; set; }

        /// <summary>
        /// length:NumChannelsInSessionGroup
        /// </summary>
        public List<UInt32> HintTrackId { get; set; }
    }
}
