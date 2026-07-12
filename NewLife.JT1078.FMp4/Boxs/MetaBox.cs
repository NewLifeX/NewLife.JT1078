using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>元数据 Box。包含媒体元数据的容器</summary>
    public class MetaBox : FullBox, IFMp4MessagePackFormatter
    {
        public MetaBox(String handlerType, Byte version = 0, UInt32 flags = 0) : base("meta", version, flags) => HandlerType = handlerType;

        public String HandlerType { get; set; }
        public HandlerBox TheHandler { get; set; }
        public DataInformationBox FileLocations { get; set; }
        public ItemLocationBox ItemLLocations { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            End(ref writer);
        }
    }
}