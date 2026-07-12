using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>子轨道信息 Box。描述子轨道的属性和切换组</summary>
    public class SubTrackInformationBox : FullBox, IFMp4MessagePackFormatter
    {
        public SubTrackInformationBox(Byte version = 0, UInt32 flags = 0) : base("stri", version, flags) { }

        public UInt16 SwitchGroup { get; set; }
        public UInt16 AlternateGroup { get; set; }
        public UInt32 SubTrackID { get; set; }
        public List<UInt32> AttributeList { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(SwitchGroup);
            writer.WriteUInt16(AlternateGroup);
            writer.WriteUInt32(SubTrackID);
            if (AttributeList != null)
                foreach (var attr in AttributeList) writer.WriteUInt32(attr);
            End(ref writer);
        }
    }
}