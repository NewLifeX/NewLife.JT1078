using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>空媒体头 Box。用于无明确媒体类型的轨道</summary>
    public class NullMediaHeaderBox : FullBox, IFMp4MessagePackFormatter
    {
        public NullMediaHeaderBox(Byte version = 0, UInt32 flags = 0) : base("nmhd", version, flags) { }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            End(ref writer);
        }
    }
}
