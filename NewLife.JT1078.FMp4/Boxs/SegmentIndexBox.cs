using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// sidx
    /// </summary>
    public class SegmentIndexBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// sidx
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SegmentIndexBox(Byte version, UInt32 flags = 0) : base("sidx", version, flags) => ReferencedSizePositions = new List<Int32>();

        /// <summary>
        /// 
        /// </summary>
        public UInt32 ReferenceID { get; set; } = 1;

        /// <summary>
        /// 
        /// </summary>
        public UInt32 Timescale { get; set; } = 1000;

        /// <summary>
        /// pts
        /// if(version==0) 
        /// version==0 32 bit
        /// version>0 64 bit
        /// </summary>
        public UInt64 EarliestPresentationTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public UInt64 FirstOffset { get; set; } = 0;

        private UInt16 Reserved { get; set; }

        //private ushort ReferenceCount { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List<SegmentIndex> SegmentIndexs { get; set; }

        public List<Int32> ReferencedSizePositions { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(ReferenceID);
            writer.WriteUInt32(Timescale);
            if (Version == 0)
            {
                writer.WriteUInt32((UInt32)EarliestPresentationTime);
                writer.WriteUInt32((UInt32)FirstOffset);
            }
            else
            {
                writer.WriteUInt64(EarliestPresentationTime);
                writer.WriteUInt64(FirstOffset);
            }
            writer.WriteUInt16(Reserved);
            if (SegmentIndexs != null && SegmentIndexs.Count > 0)
            {
                writer.WriteUInt16((UInt16)SegmentIndexs.Count);
                foreach (var si in SegmentIndexs)
                {
                    if (si.ReferenceType)
                    {
                        ReferencedSizePositions.Add(writer.GetCurrentPosition());
                        writer.WriteUInt32(si.ReferencedSize | 0x80000000);
                    }
                    else
                    {
                        ReferencedSizePositions.Add(writer.GetCurrentPosition());
                        writer.WriteUInt32(si.ReferencedSize);
                    }
                    writer.WriteUInt32(si.SubsegmentDuration);
                    if (si.StartsWithSAP)
                    {
                        writer.WriteUInt32((si.SAPDeltaTime | 0x80000000 | (UInt32)(si.SAPType << 28 & 0x70000000)));
                    }
                    else
                    {
                        writer.WriteUInt32((si.SAPDeltaTime | (UInt32)((si.SAPType << 28) & 0x70000000)));
                    }
                }
            }
            else
            {
                writer.WriteUInt16(0);
            }
            End(ref writer);
        }

        public class SegmentIndex
        {
            /// <summary>
            /// 4byte 32 - 1
            /// </summary>
            public Boolean ReferenceType { get; set; } = false;
            /// <summary>
            /// 4byte 32 - 31
            /// ReferencedSize=(moof size) + (mdat size)
            /// </summary>
            public UInt32 ReferencedSize { get; set; } = 0;
            /// <summary>
            /// 
            /// </summary>
            public UInt32 SubsegmentDuration { get; set; }
            /// <summary>
            /// 4byte 32 - 1
            /// </summary>
            public Boolean StartsWithSAP { get; set; } = true;
            /// <summary>
            /// 4byte 32 - 3
            /// </summary>
            public Byte SAPType { get; set; } = 1;
            /// <summary>
            /// 4byte 32 - 28
            /// </summary>
            public UInt32 SAPDeltaTime { get; set; } = 0;
        }
    }
}