using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>
    /// sdtp
    /// </summary>
    public class SampleDependencyTypeBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>
        /// sdtp
        /// </summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public SampleDependencyTypeBox(Byte version = 0, UInt32 flags = 0) : base("sdtp", version, flags)
        {
        }

        /// <summary>
        /// is taken from the sample_count in the Sample Size Box ('stsz') or Compact Sample Size Box(‘stz2’).
        /// </summary>
        public List<SampleDependencyType> SampleDependencyTypes { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            if (SampleDependencyTypes != null && SampleDependencyTypes.Count > 0)
            {
                foreach (var item in SampleDependencyTypes)
                {
                    writer.WriteByte((Byte)(item.IsLeading << 2 |
                        item.SampleDependsOn |
                        item.SampleIsDependedOn << 6 |
                        item.SampleHasRedundancy << 4 |
                        item.IsNonSync));
                }
            }
            End(ref writer);
        }

        public class SampleDependencyType
        {
            public Byte IsLeading { get; set; }
            public Byte SampleDependsOn { get; set; }
            public Byte SampleIsDependedOn { get; set; }
            public Byte SampleHasRedundancy { get; set; }
            //public byte DegradPrio { get; set; }
            public Byte IsNonSync { get; set; }
            //public byte PaddingValue { get; set; }
        }
    }
}