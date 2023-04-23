namespace NewLife.JT1078.FMp4
{
    public class ShadowSyncSampleBox : FullBox
    {
        public ShadowSyncSampleBox(Byte version, UInt32 flags) : base("stsh", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public List<ShadowSyncSampleInfo> ShadowSyncSampleInfos { get; set; }

        public class ShadowSyncSampleInfo
        {
            public UInt32 ShadowedSampleNumber { get; set; }
            public UInt32 SyncSampleNumber { get; set; }
        }
    }
}