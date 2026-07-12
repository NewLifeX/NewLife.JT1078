using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>分区条目 Box。文件分区表中的分区条目容器</summary>
    public class PartitionEntryBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public PartitionEntryBox() : base("paen") { }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}
