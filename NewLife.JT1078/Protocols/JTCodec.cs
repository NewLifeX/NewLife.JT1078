using System.Buffers;
using NewLife.Data;
using NewLife.Messaging;

namespace NewLife.JT1078.Protocols
{
    /// <summary>JT1078帧编解码器</summary>
    /// <remarks>
    /// JT/T 1078 RTP 报文帧格式：4字节魔数 01cd + 固定头 + 2字节负载长度 + 负载。
    /// 无状态，可跨连接共享；由 <see cref="NewLife.Net.NetServer.Protocol"/> 启用协议模式后自动定界。
    /// </remarks>
    public class JTCodec : IMessageCodec
    {
        #region 属性
        /// <summary>最大负载长度。协议长度字段为2字节</summary>
        public Int32 MaxBodyLength { get; set; } = UInt16.MaxValue;
        #endregion

        #region 解析
        /// <summary>定界并构造消息</summary>
        /// <param name="buffer">帧首窗口（只读序列，可跨段）</param>
        /// <returns>解析结果；头部数据不足时返回 null</returns>
        public ParseResult? TryParse(ReadOnlySequence<Byte> buffer)
        {
            // 固定头16字节，先探测数据类型以确定完整头长
            if (buffer.Length < 16) return null;

            Span<Byte> head = stackalloc Byte[30];
            if (!TryCopy(buffer, head[..16])) return null;

            if (head[0] != (Byte)'0' || head[1] != (Byte)'1' || head[2] != (Byte)'c' || head[3] != (Byte)'d')
                return new ParseResult { Invalid = true };

            var dataType = (DataTypes)(head[15] >> 4);
            var headerSize = 16;
            if (dataType != DataTypes.Transparent) headerSize += 8;
            if (dataType <= DataTypes.VideoBFrame) headerSize += 4;
            headerSize += 2;

            if (buffer.Length < headerSize) return null;
            if (!TryCopy(buffer, head[..headerSize])) return null;

            var bodyLength = (head[headerSize - 2] << 8) | head[headerSize - 1];
            if (bodyLength > MaxBodyLength) return new ParseResult { Invalid = true };

            var msg = new JTMessage();
            if (!msg.Read(new ArrayPacket(head[..headerSize].ToArray())))
            {
                msg.TryDispose();
                return new ParseResult { Invalid = true };
            }

            return new ParseResult { Message = msg, HeaderSize = headerSize, BodyLength = bodyLength };
        }

        /// <summary>从只读序列拷贝指定字节数到目标缓冲。跨段顺序读取，不物化整帧</summary>
        /// <param name="buffer">只读序列</param>
        /// <param name="dest">目标缓冲</param>
        /// <returns>序列长度不足时返回 false</returns>
        private static Boolean TryCopy(in ReadOnlySequence<Byte> buffer, Span<Byte> dest)
        {
            if (buffer.Length < dest.Length) return false;

            var pos = buffer.Start;
            var filled = 0;
            while (filled < dest.Length)
            {
                if (!buffer.TryGet(ref pos, out var mem, advance: false)) return false;

                var count = Math.Min(mem.Length, dest.Length - filled);
                mem.Span.Slice(0, count).CopyTo(dest.Slice(filled));

                filled += count;
                pos = buffer.GetPosition(count, pos);
            }

            return true;
        }
        #endregion

        #region 构建
        /// <summary>整帧构建（头 + 内存体）</summary>
        /// <param name="message">消息（体须为内存模式）</param>
        /// <returns>整帧数据包</returns>
        /// <exception cref="ArgumentException">消息类型不支持</exception>
        public IOwnerPacket Build(IMessage message)
        {
            if (message is not JTMessage msg) throw new ArgumentException(L.T("The message must be JTMessage."), nameof(message));

            return (IOwnerPacket)msg.ToOwnerPacket();
        }

        /// <summary>仅构建头部，声明消息体长度</summary>
        /// <param name="message">消息（提供头部字段）</param>
        /// <param name="bodyLength">消息体字节数</param>
        /// <returns>头部数据包</returns>
        /// <exception cref="ArgumentException">消息类型不支持</exception>
        public IOwnerPacket BuildHeader(IMessage message, Int64 bodyLength)
        {
            if (message is not JTMessage msg) throw new ArgumentException(L.T("The message must be JTMessage."), nameof(message));

            return (IOwnerPacket)msg.ToHeaderPacket(bodyLength);
        }
        #endregion
    }
}
