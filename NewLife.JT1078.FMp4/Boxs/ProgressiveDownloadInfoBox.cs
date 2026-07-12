using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>渐进式下载信息 Box。包含下载速率和初始延迟信息</summary>
    public class ProgressiveDownloadInfoBox : FullBox, IFMp4MessagePackFormatter
    {
        public ProgressiveDownloadInfoBox(Byte version = 0, UInt32 flags = 0) : base("pdin", version, flags) { }

        public List<ProgressiveDownloadInfo> ProgressiveDownloadInfos { get; set; }

        public class ProgressiveDownloadInfo
        {
            public UInt16 Rate { get; set; }
            public UInt16 InitialDelay { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (ProgressiveDownloadInfos != null)
            {
                foreach (var info in ProgressiveDownloadInfos)
                {
                    writer.WriteUInt16(info.Rate);
                    writer.WriteUInt16(info.InitialDelay);
                }
            }
            End(ref writer);
        }
    }
}