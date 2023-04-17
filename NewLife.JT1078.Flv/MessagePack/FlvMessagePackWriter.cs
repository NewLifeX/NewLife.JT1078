using System.Buffers.Binary;

namespace NewLife.JT1078.Flv.MessagePack;

internal ref partial struct FlvMessagePackWriter
{
    private FlvBufferWriter writer;
    public FlvMessagePackWriter(Span<Byte> buffer) => writer = new FlvBufferWriter(buffer);
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

    public Int32 GetCurrentPosition() => writer.WrittenCount;
}
