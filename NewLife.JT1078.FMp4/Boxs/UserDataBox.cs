using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    public class UserDataBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public UserDataBox() : base("udta") { }

        public Byte[] Data { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);

            if (Data != null && Data.Length > 0) writer.WriteArray(Data);

            End(ref writer);
        }
    }
}