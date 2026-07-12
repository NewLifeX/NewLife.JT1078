using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>合成解码时间偏移 Box。记录合成时间与解码时间之间的偏移关系</summary>
    public class CompositionToDecodeBox : FullBox, IFMp4MessagePackFormatter
    {
        public CompositionToDecodeBox(Byte version = 0, UInt32 flags = 0) : base("cslg", version, flags) { }

        public Int32 CompositionToDTSShift { get; set; }
        public Int32 LeastDecodeToDisplayDelta { get; set; }
        public Int32 GreatestDecodeToDisplayDelta { get; set; }
        public Int32 CompositionStartTime { get; set; }
        public Int32 CompositionEndTime { get; set; }

        /// <summary>写入序列化数据</summary>
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteInt32(CompositionToDTSShift);
            writer.WriteInt32(LeastDecodeToDisplayDelta);
            writer.WriteInt32(GreatestDecodeToDisplayDelta);
            writer.WriteInt32(CompositionStartTime);
            writer.WriteInt32(CompositionEndTime);
            End(ref writer);
        }
    }
}