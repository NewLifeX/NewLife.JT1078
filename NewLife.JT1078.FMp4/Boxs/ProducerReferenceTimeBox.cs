using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>生产者参考时间 Box。记录媒体生产时间戳</summary>
    public class ProducerReferenceTimeBox : FullBox, IFMp4MessagePackFormatter
    {
        public ProducerReferenceTimeBox(Byte version, UInt32 flags) : base("prft", version, flags) { }

        public UInt16 ReferenceTrackID { get; set; }
        public UInt32 NtpTimestamp { get; set; }
        /// <summary>if (version==0)</summary>
        public UInt32 MediaTime { get; set; }
        public UInt64 MediaTimeLagre { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt16(ReferenceTrackID);
            writer.WriteUInt32(NtpTimestamp);
            if (Version == 0)
                writer.WriteUInt32(MediaTime);
            else
                writer.WriteUInt64(MediaTimeLagre);
            End(ref writer);
        }
    }
}