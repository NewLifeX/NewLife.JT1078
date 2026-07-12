using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>组 ID 到名称映射 Box。将组 ID 映射为可读名称</summary>
    public class GroupIdToNameBox : FullBox, IFMp4MessagePackFormatter
    {
        public GroupIdToNameBox(Byte version = 0, UInt32 flags = 0) : base("gitn", version, flags) { }

        public UInt16 EntryCount { get; set; }
        public List<GroupIdToNameInfo> GroupIdToNameInfos { get; set; }

        public class GroupIdToNameInfo
        {
            public UInt32 GroupID { get; set; }
            public String GroupName { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (GroupIdToNameInfos != null && GroupIdToNameInfos.Count > 0)
            {
                EntryCount = (UInt16)GroupIdToNameInfos.Count;
                writer.WriteUInt16(EntryCount);
                foreach (var info in GroupIdToNameInfos)
                {
                    writer.WriteUInt32(info.GroupID);
                    if (!String.IsNullOrEmpty(info.GroupName)) writer.WriteUTF8(info.GroupName);
                    writer.WriteByte(0);
                }
            }
            else writer.WriteUInt16(0);
            End(ref writer);
        }
    }
}