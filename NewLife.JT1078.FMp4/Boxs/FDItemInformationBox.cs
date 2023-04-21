namespace NewLife.JT1078.FMp4
{
    public class FDItemInformationBox : FullBox
    {
        public FDItemInformationBox(Byte version, UInt32 flags) : base("fiin", version, flags) { }

        public UInt16 EntryCount { get; set; }

        /// <summary>length:EntryCount</summary>
        public List<PartitionEntryBox> PartitionEntries { get; set; }

        public FDSessionGroupBox SessionInfo { get; set; }

        public GroupIdToNameBox GroupIdToName { get; set; }
    }
}
