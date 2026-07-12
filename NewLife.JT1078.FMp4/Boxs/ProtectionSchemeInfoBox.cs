using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>保护方案信息 Box。包含 DRM 保护方案信息的容器</summary>
    public class ProtectionSchemeInfoBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public ProtectionSchemeInfoBox(String fmt) : base("sinf") => OriginalFormatBox = new OriginalFormatBox(fmt);

        public OriginalFormatBox OriginalFormatBox { get; set; }
        public SchemeTypeBox SchemeTypeBox { get; set; }
        public SchemeInformationBox SchemeInformationBox { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            OriginalFormatBox?.ToBuffer(ref writer);
            SchemeTypeBox?.ToBuffer(ref writer);
            SchemeInformationBox?.ToBuffer(ref writer);
            End(ref writer);
        }
    }
}