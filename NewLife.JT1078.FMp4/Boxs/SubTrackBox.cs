using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>子轨道 Box。包含子轨道定义的容器</summary>
    public class SubTrackBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public SubTrackBox() : base("strk") { }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}