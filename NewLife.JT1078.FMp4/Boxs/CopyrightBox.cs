namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public class CopyrightBox : FullBox
    {
        public CopyrightBox(Byte version = 0, UInt32 flags = 0) : base("cprt", version, flags) { }

        /// <summary>
        /// 16-1
        /// </summary>
        public Boolean Pad { get; set; }

        /// <summary>
        /// 16-15
        /// ISO-639-2/T language code
        /// </summary>
        public Byte Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public String Notice { get; set; }
    }
}
