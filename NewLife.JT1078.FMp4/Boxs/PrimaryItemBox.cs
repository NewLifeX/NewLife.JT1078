using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>主条目 Box。标识元数据中的主要条目</summary>
    public class PrimaryItemBox : FullBox, IFMp4MessagePackFormatter
    {
        public PrimaryItemBox(Byte version = 0, UInt32 flags = 0) : base("pitm", version, flags) { }

        public UInt16 ItemID { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(ItemID);
            End(ref writer);
        }
    }
}