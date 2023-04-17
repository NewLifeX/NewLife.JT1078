namespace NewLife.JT1078.Flv.Metadata
{
    public interface IAmf3Metadata
    {
        /// <summary>字段长度</summary>
        UInt16 FieldNameLength { get; set; }

        /// <summary>字段名称</summary>
        String FieldName { get; set; }

        /// <summary>
        /// Amf3数据类型
        /// ref:video_file_format_spec_v10.pdf scriptdatavalue  type
        /// </summary>
        Byte DataType { get; set; }

        /// <summary>对应的值</summary>
        Object Value { get; set; }

        ReadOnlySpan<Byte> ToBuffer();
    }
}
