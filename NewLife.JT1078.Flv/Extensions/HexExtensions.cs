using System;

namespace JT1078.Flv.Extensions
{
    /// <summary>
    /// 
    /// ref:"www.codeproject.com/tips/447938/high-performance-csharp-byte-array-to-hex-string-t"
    /// </summary>
    public static partial class HexExtensions
    {
        public static String ToHexString(this Byte[] source) => HexUtil.DoHexDump(source, 0, source.Length).ToUpper();

        public static Int32 WriteHexStringLittle(Byte[] bytes, Int32 offset, String data, Int32 len)
        {
            if (data == null) data = "";
            data = data.Replace(" ", "");
            var startIndex = 0;
            if (data.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                startIndex = 2;
            }
            var length = len;
            if (length == -1)
            {
                length = (data.Length - startIndex) / 2;
            }
            var noOfZero = length * 2 + startIndex - data.Length;
            if (noOfZero > 0)
            {
                data = data.Insert(startIndex, new String('0', noOfZero));
            }
            var byteIndex = 0;
            while (startIndex < data.Length && byteIndex < length)
            {
                bytes[offset + byteIndex] = Convert.ToByte(data.Substring(startIndex, 2), 16);
                startIndex += 2;
                byteIndex++;
            }
            return length;
        }

        /// <summary>
        /// 16进制字符串转16进制数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static Byte[] ToHexBytes(this String hexString)
        {
            hexString = hexString.Replace(" ", "");
            var buf = new Byte[hexString.Length / 2];
            var readOnlySpan = hexString.AsSpan();
            for (var i = 0; i < hexString.Length; i++)
            {
                if (i % 2 == 0)
                {
                    buf[i / 2] = Convert.ToByte(readOnlySpan.Slice(i, 2).ToString(), 16);
                }
            }
            return buf;
        }

        public static String ReadHexStringLittle(ReadOnlySpan<Byte> read, ref Int32 offset, Int32 len)
        {
            var source = read.Slice(offset, len);
            var hex = HexUtil.DoHexDump(read, offset, len);
            offset += len;
            return hex;
        }

        /// <summary>
        /// Copyright (c) Microsoft. All rights reserved.
        /// Licensed under the MIT license. See LICENSE file in the project root for full license information.
        /// <see cref="DotNetty.Buffers.ByteBufferUtil"/>
        /// </summary>
    }

    public static class HexUtil
    {
        private static readonly Char[] HexdumpTable = new Char[256 * 4];
        static HexUtil()
        {
            var digits = "0123456789ABCDEF".ToCharArray();
            for (var i = 0; i < 256; i++)
            {
                HexdumpTable[i << 1] = digits[(Int32)((UInt32)i >> 4 & 0x0F)];
                HexdumpTable[(i << 1) + 1] = digits[i & 0x0F];
            }
        }

        public static String DoHexDump(ReadOnlySpan<Byte> buffer, Int32 fromIndex, Int32 length)
        {
            if (length == 0)
            {
                return "";
            }
            var endIndex = fromIndex + length;
            var buf = new Char[length << 1];
            var srcIdx = fromIndex;
            var dstIdx = 0;
            for (; srcIdx < endIndex; srcIdx++, dstIdx += 2)
            {
                Array.Copy(HexdumpTable, buffer[srcIdx] << 1, buf, dstIdx, 2);
            }
            return new String(buf);
        }

        public static String DoHexDump(Byte[] array, Int32 fromIndex, Int32 length)
        {
            if (length == 0)
            {
                return "";
            }
            var endIndex = fromIndex + length;
            var buf = new Char[length << 1];
            var srcIdx = fromIndex;
            var dstIdx = 0;
            for (; srcIdx < endIndex; srcIdx++, dstIdx += 2)
            {
                Array.Copy(HexdumpTable, (array[srcIdx] & 0xFF) << 1, buf, dstIdx, 2);
            }
            return new String(buf);
        }
    }
}
