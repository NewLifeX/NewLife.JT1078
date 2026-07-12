using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>样本辅助信息偏移 Box。定义辅助信息的偏移位置</summary>
    public class SampleAuxiliaryInformationOffsetsBox : FullBox, IFMp4MessagePackFormatter
    {
        public SampleAuxiliaryInformationOffsetsBox(Byte version, UInt32 flags) : base("saio", version, flags) { }

        public UInt32 AuxInfoType { get; set; }
        public UInt32 AuxInfoTypeParameter { get; set; }
        public UInt32 EntryCount { get; set; }
        public UInt32[] Offset { get; set; }
        public UInt64[] OffsetLarge { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if ((Flags & 1) != 0)
            {
                writer.WriteUInt32(AuxInfoType);
                writer.WriteUInt32(AuxInfoTypeParameter);
            }
            if (Offset != null)
            {
                EntryCount = (UInt32)Offset.Length;
                writer.WriteUInt32(EntryCount);
                if (Version == 0)
                {
                    foreach (var o in Offset) writer.WriteUInt32(o);
                }
                else
                {
                    foreach (var o in OffsetLarge) writer.WriteUInt64(o);
                }
            }
            else writer.WriteUInt32(0);
            End(ref writer);
        }
    }
}