using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// subs
    /// </summary>
    public class SubSampleInformationBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// subs
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SubSampleInformationBox(Byte version, UInt32 flags = 0) : base("subs", version, flags) { }

        public UInt32 EntryCount { get; set; }

        public List<SubSampleInformation> SubSampleInformations { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            if (SubSampleInformations != null && SubSampleInformations.Count > 0)
            {
                foreach (var item in SubSampleInformations)
                {
                    writer.WriteUInt32(item.SampleDelta);
                    if (item.InnerSubSampleInformations != null && item.InnerSubSampleInformations.Count > 0)
                    {
                        writer.WriteUInt16((UInt16)item.InnerSubSampleInformations.Count);
                        foreach (var subItem in item.InnerSubSampleInformations)
                        {
                            if (Version == 1)
                            {
                                writer.WriteUInt32(subItem.SubsampleSize);
                            }
                            else
                            {
                                writer.WriteUInt16((UInt16)subItem.SubsampleSize);
                            }
                            writer.WriteByte(subItem.SubsamplePriority);
                            writer.WriteByte(subItem.Discardable);
                            writer.WriteUInt32(subItem.Reserved);
                        }
                    }
                    else
                    {
                        writer.WriteUInt16(0);
                    }
                }
            }
            else
            {
                writer.WriteUInt32((UInt32)SubSampleInformations.Count);
            }
        }

        public class SubSampleInformation
        {
            public UInt32 SampleDelta { get; set; }
            public UInt16 SubsampleCount { get; set; }

            public List<InnerSubSampleInformation> InnerSubSampleInformations { get; set; }

            public class InnerSubSampleInformation
            {
                /// <summary>
                /// version == 1 uint32
                /// version != 1 uint16
                /// </summary>
                public UInt32 SubsampleSize { get; set; }

                public Byte SubsamplePriority { get; set; }

                public Byte Discardable { get; set; }

                public UInt32 Reserved { get; set; }
            }
        }
    }
}