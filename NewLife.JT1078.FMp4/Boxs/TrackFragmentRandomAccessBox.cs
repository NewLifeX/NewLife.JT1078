using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>tfra</summary>
    public class TrackFragmentRandomAccessBox : FullBox, IFMp4MessagePackFormatter
    {
        /// <summary>tfra</summary>
        /// <param name="version"></param>
        /// <param name="flags"></param>
        public TrackFragmentRandomAccessBox(Byte version, UInt32 flags = 0) : base("tfra", version, flags) { }

        /// <summary></summary>
        public UInt32 TrackID { get; set; }

        /// <summary>
        /// 26bit
        /// </summary>
        private UInt32 Reserved { get; set; } = 0;

        /// <summary>
        /// 2bit 
        /// </summary>
        public Byte LengthSizeOfTrafNum { get; set; }

        /// <summary>
        /// 2bit
        /// </summary>
        public Byte LengthSizeOfTrunNum { get; set; }

        /// <summary>
        /// 2bit 
        /// </summary>
        public Byte LengthSizeOfSampleNum { get; set; }

        public UInt32 NumberOfEntry { get; set; }

        /// <summary>
        /// (moof+mdta)N
        /// </summary>
        public List<TrackFragmentRandomAccessInfo> TrackFragmentRandomAccessInfos { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            WriterFullBoxToBuffer(ref writer);
            writer.WriteUInt32(TrackID);
            writer.WriteUInt32((UInt32)(LengthSizeOfSampleNum | (LengthSizeOfTrunNum << 2) | (LengthSizeOfTrafNum << 4)));
            if (TrackFragmentRandomAccessInfos != null && TrackFragmentRandomAccessInfos.Count > 0)
            {
                writer.WriteUInt32((UInt32)TrackFragmentRandomAccessInfos.Count);
                foreach (var item in TrackFragmentRandomAccessInfos)
                {
                    if (Version == 1)
                    {
                        writer.WriteUInt64(item.Time);
                        writer.WriteUInt64(item.MoofOffset);
                    }
                    else
                    {
                        writer.WriteUInt32((UInt32)item.Time);
                        writer.WriteUInt32((UInt32)item.MoofOffset);
                    }
                    var length_size_of_traf_num = LengthSizeOfTrafNum + 1;
                    ControlSizeOf(ref writer, item.TrafNumber, length_size_of_traf_num);
                    var length_size_of_trun_num = LengthSizeOfTrunNum + 1;
                    ControlSizeOf(ref writer, item.TrunNumber, length_size_of_trun_num);
                    var length_size_of_sample_num = LengthSizeOfSampleNum + 1;
                    ControlSizeOf(ref writer, item.SampleNumber, length_size_of_sample_num);
                }
            }
            else
            {
                writer.WriteUInt32(0);
            }
            End(ref writer);
        }

        private void ControlSizeOf(ref FMp4MessagePackWriter writer, UInt32 value, Int32 length)
        {
            switch (length)
            {
                case 1:
                    writer.WriteByte((Byte)value);
                    break;
                case 2:
                    writer.WriteUInt16((UInt16)value);
                    break;
                case 3:
                    writer.WriteUInt24(value);
                    break;
                case 4:
                    writer.WriteUInt32(value);
                    break;
            }
        }

        public class TrackFragmentRandomAccessInfo
        {
            public UInt64 Time { get; set; }

            /// <summary>需要定位到当前 moof offset</summary>
            public UInt64 MoofOffset { get; set; }

            public UInt32 TrafNumber { get; set; }

            public UInt32 TrunNumber { get; set; }

            public UInt32 SampleNumber { get; set; }
        }
    }
}