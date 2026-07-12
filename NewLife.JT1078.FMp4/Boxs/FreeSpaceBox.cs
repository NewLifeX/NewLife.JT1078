using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    public abstract class FreeSpaceBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public FreeSpaceBox(String boxType) : base(boxType) { }

        /// <summary>填充值</summary>
        public Byte FillValue { get; set; }

        /// <summary>填充数量</summary>
        public Int32 FillCount { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            for (var i = 0; i < FillCount; i++) writer.WriteByte(FillValue);
            End(ref writer);
        }
    }
}
