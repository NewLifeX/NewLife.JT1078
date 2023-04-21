namespace NewLife.JT1078.FMp4;

/// <summary></summary>
public class ProgressiveDownloadInfoBox : FullBox
{
    public ProgressiveDownloadInfoBox(Byte version = 0, UInt32 flags = 0) : base("pdin", version, flags) { }

    public List<ProgressiveDownloadInfo> ProgressiveDownloadInfos { get; set; }

    public class ProgressiveDownloadInfo
    {
        public UInt16 Rate { get; set; }
        public UInt16 InitialDelay { get; set; }
    }
}
