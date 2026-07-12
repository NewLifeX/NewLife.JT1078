using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>方案信息 Box。包含特定方案数据的容器</summary>
    public class SchemeInformationBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public SchemeInformationBox() : base("schi") { }

        public List<Mp4Box> SchemeSpecificData { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}