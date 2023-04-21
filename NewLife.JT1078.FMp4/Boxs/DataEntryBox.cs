using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    public abstract class DataEntryBox : FullBox, IFMp4MessagePackFormatter
    {
        public DataEntryBox(String boxType, Byte version, UInt32 flags) : base(boxType, version, flags) { }

        public abstract void ToBuffer(ref FMp4MessagePackWriter writer);
    }
}