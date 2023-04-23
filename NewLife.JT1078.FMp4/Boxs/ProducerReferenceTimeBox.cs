namespace NewLife.JT1078.FMp4
{
    public class ProducerReferenceTimeBox : FullBox
    {
        public ProducerReferenceTimeBox(Byte version, UInt32 flags) : base("prft", version, flags) { }

        public UInt16 ReferenceTrackID { get; set; }

        public UInt32 NtpTimestamp { get; set; }

        /// <summary>
        /// if (version==0)
        /// </summary>
        public UInt32 MediaTime { get; set; }

        public UInt64 MediaTimeLagre { get; set; }
    }
}