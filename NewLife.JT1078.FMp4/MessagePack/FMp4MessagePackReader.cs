using System.Buffers.Binary;
using System.Text;

namespace NewLife.JT1078.FMp4.MessagePack
{
    public ref struct FMp4MessagePackReader
    {
        public ReadOnlySpan<Byte> Reader { get; private set; }

        public ReadOnlySpan<Byte> SrcBuffer { get; }

        public Int32 ReaderCount { get; private set; }

        public FMp4MessagePackReader(ReadOnlySpan<Byte> srcBuffer)
        {
            SrcBuffer = srcBuffer;
            ReaderCount = 0;
            Reader = srcBuffer;
        }

        public UInt16 ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(GetReadOnlySpan(2));

        /// <summary>
        /// 
        /// <see cref="https://github.com/sannies/JT1078.FMp4/blob/master/isoparser/src/main/java/org/JT1078.FMp4/tools/IsoTypeReader.java"/>
        /// </summary>
        /// <param name="language"></param>
        public String ReadIso639()
        {
            Int32 bits = ReadUInt16();
            var sb = new StringBuilder();
            for (var i = 0; i < 3; i++)
            {
                var c = (bits >> (2 - i) * 5) & 0x1f;
                sb.Append((Char)(c + 0x60));
            }
            return sb.ToString();
        }

        private ReadOnlySpan<Byte> GetReadOnlySpan(Int32 count)
        {
            ReaderCount += count;
            return Reader.Slice(ReaderCount - count);
        }
    }
}