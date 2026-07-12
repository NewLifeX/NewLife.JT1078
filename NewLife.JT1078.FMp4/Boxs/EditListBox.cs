using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>编辑列表 Box。定义轨道的编辑分段列表</summary>
    public class EditListBox : FullBox, IFMp4MessagePackFormatter
    {
        public EditListBox(Byte version, UInt32 flags = 0) : base("elst", version, flags) { }

        public UInt32 EntryCount { get; set; }
        public UInt64 SegmentDurationLarge { get; set; }
        public Int64 MediaTimeLarge { get; set; }
        public UInt32 SegmentDuration { get; set; }
        public Int32 MediaTime { get; set; }
        public Int16 MediaRateInteger { get; set; }
        public Int16 MediaRateFraction { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            EntryCount = 1;
            writer.WriteUInt32(EntryCount);
            if (Version == 1)
            {
                writer.WriteUInt64(SegmentDurationLarge);
                writer.WriteUInt64((UInt64)MediaTimeLarge);
            }
            else
            {
                writer.WriteUInt32(SegmentDuration);
                writer.WriteUInt32((UInt32)MediaTime);
            }
            writer.WriteInt16(MediaRateInteger);
            writer.WriteInt16(MediaRateFraction);
            End(ref writer);
        }
    }
}