namespace NewLife.JT1078.FMp4.Buffers;

/// <summary>
/// <see cref="System.Buffers.Writer"/>
/// </summary>
internal ref partial struct FMp4BufferWriter
{
    private readonly Span<Byte> _buffer;
    public FMp4BufferWriter(Span<Byte> buffer)
    {
        _buffer = buffer;
        WrittenCount = 0;
    }

    public Span<Byte> Free => _buffer[WrittenCount..];

    public Span<Byte> Written => _buffer[..WrittenCount];

    public Int32 WrittenCount { get; private set; }

    public void Advance(Int32 count) => WrittenCount += count;
}
