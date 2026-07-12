using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>轨道选择 Box。定义轨道切换组和选择属性</summary>
    public class TrackSelectionBox : FullBox, IFMp4MessagePackFormatter
    {
        public TrackSelectionBox(Byte version = 0, UInt32 flags = 0) : base("tsel", version, flags) { }

        public UInt32 SwitchGroup { get; set; }

        public List<UInt32> AttributeList { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(SwitchGroup);
            if (AttributeList != null)
                foreach (var attr in AttributeList) writer.WriteUInt32(attr);
            End(ref writer);
        }
    }
}