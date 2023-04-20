namespace NewLife.JT1078.FMp4
{

    public abstract class FullBox : Mp4Box
    {
        public FullBox(String boxType, Byte version, UInt32 flags) : base(boxType)
        {
            Version = version;
            Flags = flags;
        }

        /// <summary>
        /// unsigned int(8)
        /// </summary>
        public Byte Version { get; set; }

        /// <summary>
        /// bit(24)
        /// </summary>
        public UInt32 Flags { get; set; }

        /// <summary></summary>
        /// <param name="writer"></param>
        protected void WriterFullBoxToBuffer(ref FMp4MessagePackWriter writer)
        {
            writer.WriteByte(Version);
            writer.WriteUInt24(Flags);
        }
    }
}