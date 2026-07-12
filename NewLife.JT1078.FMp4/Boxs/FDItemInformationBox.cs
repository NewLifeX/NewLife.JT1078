using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>FD 条目信息 Box。包含文件分发会话信息</summary>
    public class FDItemInformationBox : FullBox, IFMp4MessagePackFormatter
    {
        public FDItemInformationBox(Byte version, UInt32 flags) : base("fiin", version, flags) { }

        public UInt16 EntryCount { get; set; }
        /// <summary>length:EntryCount</summary>
        public List<PartitionEntryBox> PartitionEntries { get; set; }
        public FDSessionGroupBox SessionInfo { get; set; }
        public GroupIdToNameBox GroupIdToName { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (PartitionEntries != null)
            {
                EntryCount = (UInt16)PartitionEntries.Count;
                writer.WriteUInt16(EntryCount);
            }
            else writer.WriteUInt16(0);
            End(ref writer);
        }
    }
}
