using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>轨道组 Box。包含轨道分组信息的容器</summary>
    public class TrackGroupBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public TrackGroupBox() : base("trgr") { }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}