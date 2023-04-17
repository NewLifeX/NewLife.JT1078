namespace NewLife.JT1078.Flv.Metadata
{
    /// <summary>
    /// <code>
    ///AVCDecoderConfigurationRecord 结构的定义：
    ///aligned(8) class AVCDecoderConfigurationRecord
    ///{
    ///unsigned int (8) configurationVersion = 1;
    ///unsigned int (8) AVCProfileIndication;
    ///unsigned int (8) profile_compatibility;
    ///unsigned int (8) AVCLevelIndication;
    ///bit(6) reserved = ‘111111’b;
    ///unsigned int (2) lengthSizeMinusOne;
    ///bit(3) reserved = ‘111’b;
    ///unsigned int (5) numOfSequenceParameterSets;
    ///for (i=0; i<numOfSequenceParameterSets; i++) {
    ///unsigned int (16) sequenceParameterSetLength ;
    ///bit(8*sequenceParameterSetLength) sequenceParameterSetNALUnit;
    ///}
    ///unsigned int (8) numOfPictureParameterSets;
    ///for (i=0; i<numOfPictureParameterSets; i++) {
    ///unsigned int (16) pictureParameterSetLength;
    ///bit(8*pictureParameterSetLength) pictureParameterSetNALUnit;
    ///}
    ///} 
    /// </code>
    /// </summary>
    public class AVCDecoderConfigurationRecord
    {
        public Byte ConfigurationVersion { get; set; } = 1;
        public Byte AVCProfileIndication { get; set; }
        public Byte ProfileCompatibility { get; set; }
        public Byte AVCLevelIndication { get; set; }
        public Int32 LengthSizeMinusOne { get; set; }
        public Int32 NumOfSequenceParameterSets { get; set; } = 0xE0 | 1;
        public Byte[] SPSBuffer { get; set; }
        public Byte NumOfPictureParameterSets { get; set; }
        public Byte[] PPSBuffer { get; set; }
        public Boolean IsPPSAndSPS => (SPSBuffer != null && SPSBuffer.Length > 0) && (PPSBuffer != null && SPSBuffer.Length > 0);

        #region Just for non-spec-conform encoders ref:org.JT1078.FMp4.boxes.iso14496.part15.AvcDecoderConfigurationRecord
        public const Int32 LengthSizeMinusOnePaddingBits = 63;
        public const Int32 NumberOfSequenceParameterSetsPaddingBits = 7;
        public const Int32 ChromaFormatPaddingBits = 31;
        public const Int32 BitDepthLumaMinus8PaddingBits = 31;
        public const Int32 BitDepthChromaMinus8PaddingBits = 31;
        #endregion
    }
}
