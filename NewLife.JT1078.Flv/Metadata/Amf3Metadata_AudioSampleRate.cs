using NewLife.JT1078.Flv.Extensions;
using System.Buffers.Binary;
using System.Text;

namespace NewLife.JT1078.Flv.Metadata
{
    public class Amf3Metadata_AudioSampleRate : IAmf3Metadata
    {
        public UInt16 FieldNameLength { get; set; }
        public String FieldName { get; set; } = "audiosamplerate";
        public Byte DataType { get; set; } = 0x00;
        public Object Value { get; set; } = 8000d;

        public ReadOnlySpan<Byte> ToBuffer()
        {
            var b1 = Encoding.ASCII.GetBytes(FieldName);
            Span<Byte> tmp = new Byte[2 + b1.Length + 1 + 8];
            BinaryPrimitives.WriteUInt16BigEndian(tmp, (UInt16)b1.Length);
            b1.CopyTo(tmp.Slice(2));
            tmp[FieldName.Length + 2] = DataType;
            this.WriteDouble(tmp.Slice(b1.Length + 3));
            return tmp;
        }
    }
}
