using NewLife.JT1078.Flv.Extensions;
using System.Buffers.Binary;
using System.Text;

namespace NewLife.JT1078.Flv.Metadata
{
    public class Amf3Metadata_Height : IAmf3Metadata
    {
        public UInt16 FieldNameLength { get; set; }
        public String FieldName { get; set; } = "height";
        public Byte DataType { get; set; } = 0x00;
        public Object Value { get; set; }

        public ReadOnlySpan<Byte> ToBuffer()
        {
            Span<Byte> tmp = new Byte[2 + 6 + 1 + 8];
            var b1 = Encoding.ASCII.GetBytes(FieldName);
            BinaryPrimitives.WriteUInt16BigEndian(tmp, (UInt16)b1.Length);
            b1.CopyTo(tmp.Slice(2));
            tmp[8] = DataType;
            this.WriteDouble(tmp.Slice(9));
            return tmp;
        }
    }
}
