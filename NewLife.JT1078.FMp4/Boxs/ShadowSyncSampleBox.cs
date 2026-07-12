using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>阴影同步样本 Box。定义非关键样本到同步样本的映射</summary>
    public class ShadowSyncSampleBox : FullBox, IFMp4MessagePackFormatter
    {
        public ShadowSyncSampleBox(Byte version, UInt32 flags) : base("stsh", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public List<ShadowSyncSampleInfo> ShadowSyncSampleInfos { get; set; }

        public class ShadowSyncSampleInfo
        {
            public UInt32 ShadowedSampleNumber { get; set; }
            public UInt32 SyncSampleNumber { get; set; }
        }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (ShadowSyncSampleInfos != null && ShadowSyncSampleInfos.Count > 0)
            {
                EntryCount = (UInt32)ShadowSyncSampleInfos.Count;
                writer.WriteUInt32(EntryCount);
                foreach (var info in ShadowSyncSampleInfos)
                {
                    writer.WriteUInt32(info.ShadowedSampleNumber);
                    writer.WriteUInt32(info.SyncSampleNumber);
                }
            }
            else writer.WriteUInt32(0);
            End(ref writer);
        }
    }
}