using NewLife.JT1078.FMp4.Buffers;
using System.Buffers.Binary;
using System.Text;

namespace NewLife.JT1078.FMp4.MessagePack
{
    public ref struct FMp4MessagePackWriter
    {
        private FMp4BufferWriter writer;

        private Int32 TrunDataOffsetPosition;

        private Int32 MfraSizePosition;
        private Int32 MoofOffsetPosition;

        public FMp4MessagePackWriter(Span<Byte> buffer)
        {
            this.writer = new FMp4BufferWriter(buffer);
            TrunDataOffsetPosition = 0;
            MfraSizePosition = 0;
            MoofOffsetPosition = 0;
        }
        public Byte[] FlushAndGetArray() => writer.Written.ToArray();
        public void WriteByte(Byte value)
        {
            var span = writer.Free;
            span[0] = value;
            writer.Advance(1);
        }

        public void WriteUInt16(UInt16 value)
        {
            BinaryPrimitives.WriteUInt16BigEndian(writer.Free, value);
            writer.Advance(2);
        }

        public void WriteInt16(Int16 value)
        {
            BinaryPrimitives.WriteInt16BigEndian(writer.Free, value);
            writer.Advance(2);
        }

        public void WriteInt32(Int32 value)
        {
            BinaryPrimitives.WriteInt32BigEndian(writer.Free, value);
            writer.Advance(4);
        }

        public void WriteUInt64(UInt64 value)
        {
            BinaryPrimitives.WriteUInt64BigEndian(writer.Free, value);
            writer.Advance(8);
        }

        public void WriteUInt32(UInt32 value)
        {
            BinaryPrimitives.WriteUInt32BigEndian(writer.Free, value);
            writer.Advance(4);
        }

        public void WriteUInt32Little(UInt32 value)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(writer.Free, value);
            writer.Advance(4);
        }

        public void WriteUInt24(UInt32 value)
        {
            var span = writer.Free;
            span[0] = (Byte)(value >> 16);
            span[1] = (Byte)(value >> 8);
            span[2] = (Byte)value;
            writer.Advance(3);
        }

        public void WriteASCII(String value)
        {
            var data = Encoding.ASCII.GetBytes(value);
            data.CopyTo(writer.Free);
            writer.Advance(data.Length);
        }

        public void WriteUTF8(String value)
        {
            var data = Encoding.UTF8.GetBytes(value);
            data.CopyTo(writer.Free);
            writer.Advance(data.Length);
        }

        public void WriteArray(ReadOnlySpan<Byte> src)
        {
            src.CopyTo(writer.Free);
            writer.Advance(src.Length);
        }
        public void Skip(Int32 count, out Int32 position)
        {
            position = writer.WrittenCount;
            var tmp = new Byte[count];
            tmp.CopyTo(writer.Free);
            writer.Advance(count);
        }
        public void WriteUInt16Return(UInt16 value, Int32 position) => BinaryPrimitives.WriteUInt16BigEndian(writer.Written.Slice(position, 2), value);

        public void WriteInt32Return(Int32 value, Int32 position) => BinaryPrimitives.WriteInt32BigEndian(writer.Written.Slice(position, 4), value);

        public void WriteUInt32Return(UInt32 value, Int32 position) => BinaryPrimitives.WriteUInt32BigEndian(writer.Written.Slice(position, 4), value);

        public void WriteUInt64Return(UInt64 value, Int32 position) => BinaryPrimitives.WriteUInt64BigEndian(writer.Written.Slice(position, 8), value);

        public void WriteByteReturn(Byte value, Int32 position) => writer.Written[position] = value;

        public Int32 GetCurrentPosition() => writer.WrittenCount;

        public void CreateTrunOffsetPosition() => TrunDataOffsetPosition = writer.WrittenCount;

        public Int32 GetTrunOffsetPosition() => TrunDataOffsetPosition;

        public void ClearTrunOffsetPosition() => TrunDataOffsetPosition = 0;

        public void CreateMfraSizePosition() => MfraSizePosition = writer.WrittenCount;

        public Int32 GetMfraSizePosition() => MfraSizePosition;

        public void ClearMfraSizePosition() => MfraSizePosition = 0;

        public void CreateMoofOffsetPosition() => MoofOffsetPosition = writer.WrittenCount;

        public Int32 GetMoofOffsetPosition() => MoofOffsetPosition;

        public void ClearMoofOffsetPosition() => MoofOffsetPosition = 0;

        /// <summary>ref</summary>
        /// <param name="language"></param>
        public void WriteIso639(String language)
        {
            var bytes = Encoding.UTF8.GetBytes(language);
            var bits = 0;
            for (var i = 0; i < 3; i++)
            {
                bits += (bytes[i] - 0x60) << (2 - i) * 5;
            }
            WriteUInt16((UInt16)bits);
        }
    }
}
