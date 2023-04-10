namespace NewLife.JT1078.H264
{
    public struct NALUHeader
    {
        public NALUHeader(Byte value)
        {
            ForbiddenZeroBit = (value & 0x80) >> 7;
            NalRefIdc = (value & 0x60) >> 5;
            NalUnitType = (NalUnitType)(value & 0x1f);
            KeyFrame = NalUnitType == NalUnitType.IDR;
        }

        public NALUHeader(ReadOnlySpan<Byte> value)
        {
            ForbiddenZeroBit = (value[0] & 0x80) >> 7;
            NalRefIdc = (value[0] & 0x60) >> 5;
            NalUnitType = (NalUnitType)(value[0] & 0x1f);
            KeyFrame = NalUnitType == NalUnitType.IDR;
        }

        public Int32 ForbiddenZeroBit { get; set; }

        public Int32 NalRefIdc { get; set; }

        public NalUnitType NalUnitType { get; set; }

        public Boolean KeyFrame { get; set; }
    }

    public enum NalUnitType : Int32
    {
        None = 0,
        SLICE = 1,
        DPA = 2,
        DPB = 3,
        DPC = 4,
        IDR = 5,
        SEI = 6,
        SPS = 7,
        PPS = 8,
        AUD = 9,
        EOSEQ = 10,
        EOSTREAM = 11,
        FILL = 12,
    }
}