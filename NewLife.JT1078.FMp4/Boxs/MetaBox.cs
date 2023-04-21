using System;

namespace NewLife.JT1078.FMp4
{
    public class MetaBox : FullBox
    {
        public MetaBox(String handlerType, Byte version = 0, UInt32 flags = 0) : base("meta", version, flags) => HandlerType = handlerType;

        public String HandlerType { get; set; }

        public HandlerBox TheHandler { get; set; }

        public DataInformationBox FileLocations { get; set; }

        public ItemLocationBox ItemLLocations { get; set; }
    }

}