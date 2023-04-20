namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public class ChunkLargeOffsetBox : FullBox
    {
        public ChunkLargeOffsetBox(Byte version = 0, UInt32 flags = 0) : base("co64", version, flags) { }

        public UInt32 EntryCount { get; set; }

        /// <summary>length:EntryCount</summary>
        public List<UInt64> ChunkOffset { get; set; }
    }
}