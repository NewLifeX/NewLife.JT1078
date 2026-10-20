using NewLife.Data;
using NewLife.JT1078.Serialization;
using NewLife.Log;
using NewLife.Messaging;
using NewLife.Serialization;
using System.Runtime.Serialization;

namespace NewLife.JT1078.Protocols
{
    /// <summary>JT1078报文消息</summary>
    public class JTMessage : Message, IAccessor
    {
        #region 属性
        /// <summary>协议版本号</summary>
        /// <remarks>
        /// 最高2位是版本，整体 0b10000001=129
        /// 
        /// V  - 2 - 固定为2
        /// P  - 1 - 固定为0
        /// X  - 1 - RTP头是否需要扩展位，固定为0
        /// CC - 4 - 固定为1
        /// 01000001
        /// </remarks>
        public Byte Version { get; set; }

        /// <summary>音频编码类型</summary>
        public AVTypes Type { get; set; }

        /// <summary>完整帧</summary>
        /// <remarks>确定是否是完整数据帧的边界</remarks>
        public Boolean Completed { get; set; }

        /// <summary>消息流水号</summary>
        /// <remarks>初始为0，每发送一个 RTP 数据包，序列号加 1</remarks>
        public UInt16 Sequence { get; set; }

        /// <summary>SIM卡号</summary>
        /// <remarks>终端设备 SIM 卡号</remarks>
        [BCDString(6)]
        public String SIM { get; set; }

        /// <summary>逻辑通道号</summary>
        public Byte Channel { get; set; }

        /// <summary>数据类型</summary>
        /// <remarks>
        /// 0，视频 I 帧
        /// 1，视频 P 帧
        /// 2，视频 B 帧
        /// 3，音频帧
        /// 4，透传数据
        /// </remarks>
        public DataTypes DataType { get; set; }

        /// <summary>分包处理标记</summary>
        /// <remarks>
        /// 0000：原子包，不可被拆分
        /// 0001：分包处理时的第一个包
        /// 0010：分包处理是的最后一个包
        /// 0011：分包处理时间的中间包
        /// </remarks>
        public PacketFlags PacketFlag { get; set; }

        /// <summary>时间戳</summary>
        /// <remarks>标识此 RTP 数据包当前帧的相对时间，单位毫秒（ms）。当数据类型为 0100 时，则没有该字段</remarks>
        public UInt64 Timestamp { get; set; }

        /// <summary>上一个关键帧间隔</summary>
        /// <remarks>该帧与上一个关键帧之间的时间间隔，单位毫秒（ms），当数据类型为非视频帧时，则没有该字段</remarks>
        public UInt16 LastIFrameInterval { get; set; }

        /// <summary>上一个关键帧间隔</summary>
        /// <remarks>该帧与上一帧之间的时间间隔，单位毫秒（ms），当数据类型为非视频帧时，则没有该字段</remarks>
        public UInt16 LastFrameInterval { get; set; }
        #endregion

        #region 接口属性
        /// <summary>负载数据</summary>
        /// <remarks>消息体句柄视图，赋值即转移数据包所有权；置 null 表示放弃持有</remarks>
        [IgnoreDataMember]
        public new IPacket Payload
        {
            get => base.Payload;
            set => SetBody(value);
        }
        #endregion

        #region 读写
        /// <summary>解析数据流</summary>
        /// <param name="stream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public Boolean Read(Stream stream, Object context)
        {
            if (stream == null && context == null) return false;

            var reader = new Binary { Stream = stream, IsLittleEndian = false };

            // 头部标识探测
            var header = reader.ReadFixedString(4);
            if (header != "01cd") return false;

            Version = reader.ReadByte();

            var b = reader.ReadByte();
            Completed = (b & 0x80) > 0;
            Type = (AVTypes)(b & 0x7F);

            Sequence = reader.ReadUInt16();
            SIM = reader.ReadBCD(6);
            Channel = reader.ReadByte();

            b = reader.ReadByte();
            DataType = (DataTypes)(Byte)(b >> 4);
            PacketFlag = (PacketFlags)(b & 0x0F);

            if (DataType != DataTypes.Transparent) Timestamp = reader.Read<UInt64>();

            if (DataType <= DataTypes.VideoBFrame)
            {
                LastIFrameInterval = reader.ReadUInt16();
                LastFrameInterval = reader.ReadUInt16();
            }

            var rs = true;
            var len = reader.ReadUInt16();
            if (len > 0)
            {
                var retain = reader.Stream.Length - reader.Stream.Position;
                if (len > retain)
                {
                    len = (UInt16)retain;
                    //rs = false;
                }

                if (len > 0) Payload = new ArrayPacket(reader.ReadBytes(len));
            }

            return rs;
        }

        /// <summary>写入数据</summary>
        /// <param name="stream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Boolean Write(Stream stream, Object context)
        {
            if (stream == null && context == null) return false;

            WriteHeader(stream, Payload?.Total ?? 0);

            Payload?.CopyTo(stream);

            return true;
        }

        /// <summary>写入头部。长度字段使用指定负载长度，不写入负载数据</summary>
        /// <param name="stream">目标数据流</param>
        /// <param name="payloadLength">负载字节数</param>
        private void WriteHeader(Stream stream, Int64 payloadLength)
        {
            var writer = new Binary { Stream = stream, IsLittleEndian = false };

            writer.WriteFixedString("01cd", 4);

            writer.Write(Version);

            var b = (Byte)Type;
            if (Completed) b |= 0x80;
            writer.Write(b);

            writer.WriteUInt16(Sequence);
            writer.WriteBCD(SIM ?? "", 6);
            writer.Write(Channel);

            b = (Byte)(((Byte)DataType << 4) | (Byte)PacketFlag);
            writer.Write(b);

            if (DataType != DataTypes.Transparent) writer.Write(Timestamp);

            if (DataType <= DataTypes.VideoBFrame)
            {
                writer.WriteUInt16(LastIFrameInterval);
                writer.WriteUInt16(LastFrameInterval);
            }

            writer.WriteUInt16((UInt16)payloadLength);
        }

        /// <summary>读取数据包</summary>
        /// <param name="pk"></param>
        /// <returns></returns>
        public Boolean Read(IPacket pk) => Read(pk.GetStream(), null);

        /// <summary>转数据包</summary>
        /// <returns></returns>
        public IPacket ToPacket() => ToOwnerPacket();

        /// <summary>转所有权数据包</summary>
        /// <returns>拥有句柄的数据包，调用方负责释放</returns>
        public IOwnerPacket ToOwnerPacket()
        {
            try
            {
                var ms = new MemoryStream();
                Write(ms, null);

                ms.Position = 0;
                return new OwnerPacket(ms, 0);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
                throw;
            }
        }

        /// <summary>构建头部数据包。长度字段使用指定负载长度，不包含负载数据</summary>
        /// <param name="payloadLength">负载字节数</param>
        /// <returns>头部数据包，调用方负责释放</returns>
        public IOwnerPacket ToHeaderPacket(Int64 payloadLength)
        {
            if (payloadLength is < 0 or > UInt16.MaxValue) throw new ArgumentOutOfRangeException(nameof(payloadLength));

            var ms = new MemoryStream();
            WriteHeader(ms, payloadLength);

            ms.Position = 0;
            return new OwnerPacket(ms, 0);
        }
        #endregion

        #region 分包
        /// <summary>附加分包，并返回是否完整</summary>
        /// <param name="message"></param>
        /// <returns>是否完整，也就是已收到最后一个包</returns>
        public Boolean Append(JTMessage message)
        {
            if (message.PacketFlag == PacketFlags.Alone) return false;

            // 只有第一个包能合并其它包
            if (PacketFlag != PacketFlags.First) throw new Exception(L.T("JTMessage.Append need first packet"));

            if (message.Payload == null) return false;

            // 放入分包数组，注意要深度拷贝，否则可能存在网络缓冲区冲突，导致数据被修改
            Payload = Payload == null ? message.Payload.Clone() : Payload.Append(message.Payload.Clone());

            if (message.PacketFlag == PacketFlags.Last) return true;

            return false;
        }
        #endregion

        #region 辅助
        /// <summary>克隆消息。复制全部头部字段，负载深拷贝</summary>
        /// <returns>独立的新消息实例</returns>
        public JTMessage Clone()
        {
            var msg = new JTMessage
            {
                Version = Version,
                Type = Type,
                Completed = Completed,
                Sequence = Sequence,
                SIM = SIM,
                Channel = Channel,
                DataType = DataType,
                PacketFlag = PacketFlag,
                Timestamp = Timestamp,
                LastIFrameInterval = LastIFrameInterval,
                LastFrameInterval = LastFrameInterval,
                Kind = Kind,
            };

            if (Payload != null) msg.Payload = Payload.Clone();

            return msg;
        }

        /// <summary>创建响应</summary>
        /// <returns></returns>
        public override IMessage CreateReply() => new JTMessage { Reply = true, Version = Version, Type = Type, SIM = SIM, Channel = Channel };

        /// <summary>获取数据包长度</summary>
        /// <param name="pk"></param>
        /// <returns></returns>
        public static Int32 GetLength(IPacket pk)
        {
            if (pk.Total < 16 + 2) return 0;

            var dataType = (DataTypes)(pk[15] >> 4);

            // 头部长度
            var len = 4 + 1 + 1 + 2 + 6 + 1 + 1;
            if (dataType != DataTypes.Transparent) len += 8;
            if (dataType <= DataTypes.VideoBFrame) len += 2 + 2;

            var pk2 = pk.Slice(len, pk.Total - len);
            if (pk2.Total < 2) return 0;

            // 手动读取 2 字节大端长度 (替换已过时的 Packet.ReadUInt16)
            len += (pk2[0] << 8) | pk2[1];

            len += 2;

            return len;
        }

        /// <summary>获取数据包长度（GetLength2 委托，使用 Span 提升性能）</summary>
        /// <param name="span">数据片段</param>
        /// <returns>完整消息长度，返回0表示数据不足</returns>
        public static Int32 GetLength2(ReadOnlySpan<Byte> span)
        {
            if (span.Length < 16 + 2) return 0;

            var dataType = (DataTypes)(span[15] >> 4);

            // 头部长度
            var len = 4 + 1 + 1 + 2 + 6 + 1 + 1;
            if (dataType != DataTypes.Transparent) len += 8;
            if (dataType <= DataTypes.VideoBFrame) len += 2 + 2;

            if (span.Length < len + 2) return 0;

            // 手动读取 2 字节大端长度
            len += (span[len] << 8) | span[len + 1];
            len += 2;

            return len;
        }

        public String GetKey() => $"{SIM}_{Channel}";

        /// <summary>已重写。返回友好文本</summary>
        /// <returns></returns>
        public override String ToString() => $"[{Type}] {SIM}-{Channel} Seq={Sequence} [{Payload?.Total}]";
        #endregion
    }
}