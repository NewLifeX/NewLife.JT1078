using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>子轨道定义 Box。包含子轨道具体信息的容器</summary>
    public class SubTrackDefinitionBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public SubTrackDefinitionBox() : base("strd") { }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}