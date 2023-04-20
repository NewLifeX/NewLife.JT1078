using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public abstract class Mp4Box
    {
        //public const string UUID = "uuid";

        private const Int32 FixedSizeLength = 4;

        public Mp4Box(String boxType) => BoxType = boxType;

        ///// <summary>
        ///// 不考虑自定义
        ///// </summary>
        //public Mp4Box(string boxType, string extendedType)
        //{
        //    BoxType = boxType;
        //    if (boxType == UUID)
        //    {
        //       UserType = extendedType;
        //    }
        //}

        /// <summary>盒子大小</summary>
        public UInt32 Size { get; set; }

        ///// <summary>
        ///// 不考虑size==1
        ///// size==1
        ///// </summary>
        //public ulong SizeLarge { get; set; }

        /// <summary>
        /// 盒子类型
        /// </summary>
        public String BoxType { get; set; }

        ///// <summary>
        ///// 不考虑boxtype==‘uuid'
        ///// boxtype==‘uuid’
        ///// int(8)[16]
        ///// </summary>
        //public string UserType { get; set; }

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
