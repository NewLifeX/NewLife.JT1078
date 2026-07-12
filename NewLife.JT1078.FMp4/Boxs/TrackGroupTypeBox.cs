using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>轨道组类型 Box。标识轨道所属的分组</summary>
    public class TrackGroupTypeBox : FullBox, IFMp4MessagePackFormatter
    {
        public TrackGroupTypeBox(String boxType, Byte version = 0, UInt32 flags = 0) : base(boxType, version, flags) { }

        public UInt32 TrackGroupId { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(TrackGroupId);
            End(ref writer);
        }
    }
}