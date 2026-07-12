using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>样本辅助信息大小 Box。定义每个样本的辅助信息大小</summary>
    public class SampleAuxiliaryInformationSizesBox : FullBox, IFMp4MessagePackFormatter
    {
        public SampleAuxiliaryInformationSizesBox(Byte version = 0, UInt32 flags = 0) : base("saiz", version, flags) { }

        public UInt32 AuxInfoType { get; set; }
        public UInt32 AuxInfoTypeParameter { get; set; }
        public Byte DefaultSampleInfoSize { get; set; }
        public UInt32 SampleCount { get; set; }
        public Byte[] SampleInfoSize { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if ((Flags & 1) != 0)
            {
                writer.WriteUInt32(AuxInfoType);
                writer.WriteUInt32(AuxInfoTypeParameter);
            }
            writer.WriteByte(DefaultSampleInfoSize);
            if (DefaultSampleInfoSize == 0 && SampleInfoSize != null)
            {
                SampleCount = (UInt32)SampleInfoSize.Length;
                writer.WriteUInt32(SampleCount);
                writer.WriteArray(SampleInfoSize);
            }
            else
            {
                writer.WriteUInt32(SampleCount);
            }
            End(ref writer);
        }
    }
}
