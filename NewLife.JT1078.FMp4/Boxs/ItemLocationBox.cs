namespace NewLife.JT1078.FMp4
{
    public class ItemLocationBox : FullBox
    {
        public ItemLocationBox(Byte version, UInt32 flags = 0) : base("iloc", version, flags) { }

        /// <summary>
        /// 0, 4, 8
        /// </summary>
        public Byte OffsetSize { get; set; }

        /// <summary>
        /// 0, 4, 8
        /// </summary>
        public Byte LengthSize { get; set; }

        /// <summary>
        /// 0, 4, 8
        /// </summary>
        public Byte BaseOffsetSize { get; set; }

        /// <summary>
        /// version == 1
        /// </summary>
        public Byte IndexSize { get; set; }

        public Byte Reserved { get; set; }

        public UInt16 ItemCount { get; set; }

        public List<ItemLocation> ItemLocations { get; set; }

        public class ItemLocation
        {
            public UInt16 ItemID { get; set; }

            /// <summary>
            /// if (version == 1) 16-12
            /// </summary>
            public Byte Reserved { get; set; } = 12;
            
            /// <summary>
            /// if (version == 1) 16-4
            /// </summary>
            public Byte ConstructionMethod { get; set; }
            
            public UInt16 DataReferenceIndex { get; set; }
            
            public UInt64 BaseOffset { get; set; }
            
            public UInt16 ExtentCount { get; set; }
            
            public List<ItemLocationExtentInfo> ItemLocationExtentInfos { get; set; }
            
            public class ItemLocationExtentInfo
            {
                /// <summary>
                /// if ((version == 1) && (index_size > 0))
                /// </summary>
                public UInt64 ExtentIndex { get; set; }
                public UInt64 ExtentOffset { get; set; }
                public UInt64 ExtentLength { get; set; }
            }
        }
    }
}
