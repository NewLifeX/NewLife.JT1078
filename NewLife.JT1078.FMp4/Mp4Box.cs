using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public abstract class Mp4Box
    {
        private const Int32 FixedSizeLength = 4;

        public Mp4Box(String boxType) => BoxType = boxType;

        /// <summary>盒子大小</summary>
        public UInt32 Size { get; set; }

        /// <summary>盒子类型</summary>
        public String BoxType { get; set; }

        /// <summary></summary>
        public Int32 SizePosition;

        /// <summary></summary>
        /// <param name="writer"></param>
        public void Start(ref FMp4MessagePackWriter writer)
        {
            writer.Skip(FixedSizeLength, out SizePosition);
            writer.WriteASCII(BoxType);
        }

        /// <summary></summary>
        /// <param name="writer"></param>
        public void End(ref FMp4MessagePackWriter writer) => writer.WriteUInt32Return((UInt32)(writer.GetCurrentPosition() - SizePosition), SizePosition);
    }
}
