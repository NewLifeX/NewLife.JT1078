using System;

namespace NewLife.JT1078.FMp4
{
    public abstract class FreeSpaceBox : Mp4Box
    {
        public FreeSpaceBox(String boxType) : base(boxType) { }

        /// <summary>填充值</summary>
        public Byte FillValue { get; set; }

        /// <summary>填充数量</summary>
        public Int32 FillCount { get; set; }
    }
}
