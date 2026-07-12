using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>FD 会话组 Box。描述文件分发会话组</summary>
    public class FDSessionGroupBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public FDSessionGroupBox() : base("segr") { }

        public UInt16 NumSessionGroups { get; set; }

        public class FDSessionGroupInfo
        {
            public Byte EntryCount { get; set; }
            public List<UInt32> GroupIDs { get; set; }
            public UInt16 NumChannelsInSessionGroup { get; set; }
            public List<UInt32> HintTrackId { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}
