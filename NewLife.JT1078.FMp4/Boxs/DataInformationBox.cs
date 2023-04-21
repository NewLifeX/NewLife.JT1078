using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>dinf</summary>
    public class DataInformationBox : Mp4Box, IFMp4MessagePackFormatter
    {
        /// <summary>dinf</summary>
        public DataInformationBox() : base("dinf") { }

        /// <summary>dref</summary>
        public DataReferenceBox DataReferenceBox { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            DataReferenceBox.ToBuffer(ref writer);
            End(ref writer);
        }
    }
}