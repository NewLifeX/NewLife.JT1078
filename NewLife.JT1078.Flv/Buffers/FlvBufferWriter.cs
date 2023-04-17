using System;

namespace NewLife.JT1078.Flv
{
    /// <summary>
    /// <see cref="System.Buffers.Writer"/>
    /// </summary>
    internal ref partial struct FlvBufferWriter
    {
        private readonly Span<Byte> _buffer;
        public FlvBufferWriter(Span<Byte> buffer)
        {
            _buffer = buffer;
            WrittenCount = 0;
        }

        public Span<Byte> Free => _buffer.Slice(WrittenCount);

        public Span<Byte> Written => _buffer.Slice(0, WrittenCount);

        public Int32 WrittenCount { get; private set; }

        public void Advance(Int32 count) => WrittenCount += count;
    }
}
