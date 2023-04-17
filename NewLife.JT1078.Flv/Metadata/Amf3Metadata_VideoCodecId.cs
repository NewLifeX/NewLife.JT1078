using NewLife.JT1078.Flv.Extensions;
using System.Buffers.Binary;
using System.Text;

namespace NewLife.JT1078.Flv.Metadata
{
    public class Amf3Metadata_VideoCodecId : IAmf3Metadata
    {
        public UInt16 FieldNameLength { get; set; }
        public String FieldName { get; set; } = "videocodecid";
        public Byte DataType { get; set; } = 0x00;
        /// <summary>
        /// <see cref="typeof(JT1078.Flv.Enums.CodecId.AvcVideoPacke)"/>
        /// </summary>
        public Object Value { get; set; }

        public ReadOnlySpan<Byte> ToBuffer()
        {
            Span<Byte> tmp = new Byte[2 + 12 + 1 + 8];
            var b1 = Encoding.ASCII.GetBytes(FieldName);
            BinaryPrimitives.WriteUInt16BigEndian(tmp, (UInt16)b1.Length);
            b1.CopyTo(tmp.Slice(2));
            tmp[14] = DataType;
            this.WriteDouble(tmp.Slice(15));
            return tmp;
        }
    }
}
