using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>子轨道样本组 Box。将子轨道关联到样本组描述</summary>
    public class SubTrackSampleGroupBox : FullBox, IFMp4MessagePackFormatter
    {
        public SubTrackSampleGroupBox(Byte version = 0, UInt32 flags = 0) : base("stsg", version, flags) { }

        public UInt32 GroupingType { get; set; }
        public UInt16 ItemCount { get; set; }
        public List<UInt32> GroupDescriptionIndex { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(GroupingType);
            if (GroupDescriptionIndex != null && GroupDescriptionIndex.Count > 0)
            {
                ItemCount = (UInt16)GroupDescriptionIndex.Count;
                writer.WriteUInt16(ItemCount);
                foreach (var idx in GroupDescriptionIndex) writer.WriteUInt32(idx);
            }
            else writer.WriteUInt16(0);
            End(ref writer);
        }
    }
}