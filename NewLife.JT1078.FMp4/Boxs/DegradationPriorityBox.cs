using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>降解优先级 Box。每个样本的优先级值</summary>
    public class DegradationPriorityBox : FullBox, IFMp4MessagePackFormatter
    {
        public DegradationPriorityBox(Byte version = 0, UInt32 flags = 0) : base("stdp", version, flags) { }

        /// <summary>每个样本的优先级值列表，数量取自 stsz 的 sample_count</summary>
        public List<UInt16> Priorities { get; set; }

        /// <summary>写入序列化数据</summary>
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (Priorities != null)
            {
                foreach (var item in Priorities) writer.WriteUInt16(item);
            }
            End(ref writer);
        }
    }
}