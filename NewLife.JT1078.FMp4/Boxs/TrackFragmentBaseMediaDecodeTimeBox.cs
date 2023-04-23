using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// tfdt
    /// </summary>
    public class TrackFragmentBaseMediaDecodeTimeBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// tfdt
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public TrackFragmentBaseMediaDecodeTimeBox(Byte version = 0, UInt32 flags = 0) : base("tfdt", version, flags) { }

        /// <summary>
        /// 
        /// </summary>
        public UInt64 BaseMediaDecodeTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (Version == 1)
            {
                writer.WriteUInt64(BaseMediaDecodeTime);
            }
            else
            {
                writer.WriteUInt32((UInt32)BaseMediaDecodeTime);
            }
            End(ref writer);
        }
    }
}
