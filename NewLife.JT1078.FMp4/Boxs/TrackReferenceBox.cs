using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>轨道引用 Box。包含轨道间引用关系的容器</summary>
    public class TrackReferenceBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public TrackReferenceBox() : base("tref") { }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}