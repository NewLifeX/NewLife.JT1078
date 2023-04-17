using NewLife.JT1078.Flv.Metadata;

namespace NewLife.JT1078.Flv.Extensions
{
    public static class Amf3Extensions
    {
        public static void WriteDouble(this IAmf3Metadata metadata, Span<Byte> value)
        {
            var flvBuffer = BitConverter.GetBytes(Convert.ToDouble(metadata.Value)).AsSpan();
            flvBuffer.Reverse();
            flvBuffer.CopyTo(value);
        }

        public static void WriteBool(this IAmf3Metadata metadata, Span<Byte> value)
        {
            var flvBuffer = BitConverter.GetBytes(Convert.ToBoolean(metadata.Value)).AsSpan();
            flvBuffer.CopyTo(value);
        }
    }
}
