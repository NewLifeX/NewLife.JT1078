using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>条目保护 Box。包含项目保护信息的容器</summary>
    public class ItemProtectionBox : FullBox, IFMp4MessagePackFormatter
    {
        public ItemProtectionBox(Byte version = 0, UInt32 flags = 0) : base("ipro", version, flags) { }

        public UInt16 ProtectionCount { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(ProtectionCount);
            End(ref writer);
        }
    }
}
