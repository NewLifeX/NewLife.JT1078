using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4.Interfaces
{
    public interface IFMp4MessagePackFormatter
    {
        void ToBuffer(ref FMp4MessagePackWriter writer);
    }
}
