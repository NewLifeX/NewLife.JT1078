using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>轨道引用类型 Box。包含引用轨道 ID 列表</summary>
    public class TrackReferenceTypeBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public TrackReferenceTypeBox(String referenceType) : base(referenceType) { }

        public List<UInt32> TrackIDs { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            if (TrackIDs != null)
            {
                foreach (var id in TrackIDs) writer.WriteUInt32(id);
            }
            End(ref writer);
        }
    }
}