using NewLife.Data;
using NewLife.JT1078.Serialization;
using NewLife.Log;
using NewLife.Messaging;
using NewLife.Serialization;
using System.Runtime.Serialization;

namespace NewLife.JT1078.Protocols
{
    /// <summary>JT1078报文消息</summary>
    public class JTMessage : IMessage, IAccessor
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
        /// <summary>是否响应</summary>
        /// <remarks>所有终端主发的消息均应要求平台应答</remarks>
        public Boolean Reply { get; set; }

        /// <summary>是否错误响应</summary>
        [IgnoreDataMember]
        Boolean IMessage.Error { get; set; }

        /// <summary>是否单向请求。单向请求无需等待响应</summary>
        [IgnoreDataMember]
        Boolean IMessage.OneWay { get; }

        /// <summary>负载数据</summary>
        [IgnoreDataMember]
        public Packet Payload { get; set; }
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

            if (DataType != DataTypes.透传数据) Timestamp = reader.Read<UInt64>();

            if (DataType <= DataTypes.视频B帧)
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

                Payload = reader.ReadBytes(len);
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

            var writer = new Binary { Stream = stream, IsLittleEndian = false };

            writer.WriteFixedString("01cd", 4);

            writer.Write(Version);

            var b = (Byte)Type;
            if (Completed) b |= 0x80;
            writer.Write(b);

            writer.WriteUInt16(Sequence);
            writer.WriteBCD(SIM, 6);
            writer.Write(Channel);

            b = (Byte)(((Byte)DataType << 4) | (Byte)PacketFlag);
            writer.Write(b);

            if (DataType != DataTypes.透传数据) writer.Write(Timestamp);

            if (DataType <= DataTypes.视频B帧)
            {
                writer.WriteUInt16(LastIFrameInterval);
                writer.WriteUInt16(LastFrameInterval);
            }

            var len = Payload?.Total ?? 0;
            writer.WriteUInt16((UInt16)len);
            Payload?.CopyTo(stream);

            return true;
        }

        /// <summary>读取数据包</summary>
        /// <param name="pk"></param>
        /// <returns></returns>
        public Boolean Read(Packet pk) => Read(pk.GetStream(), null);

        /// <summary>转数据包</summary>
        /// <returns></returns>
        public Packet ToPacket()
        {
            try
            {
                var ms = new MemoryStream();
                if (!Write(ms, null)) return null;

                ms.Position = 0;
                return new Packet(ms);
            }
            catch (Exception ex)
            {
                XTrace.WriteException(ex);
                throw;
            }
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
            if (PacketFlag != PacketFlags.First) throw new Exception("JTMessage.Append need first packet");

            // 放入分包数组，注意要深度拷贝，否则可能存在网络缓冲区冲突，导致数据被修改
            Payload.Append(message.Payload.Clone());

            if (message.PacketFlag == PacketFlags.Last) return true;

            return false;
        }
        #endregion

        #region 辅助
        /// <summary>创建响应</summary>
        /// <returns></returns>
        public IMessage CreateReply() => new JTMessage { Reply = true, Version = Version, Type = Type, SIM = SIM };

        /// <summary>获取数据包长度</summary>
        /// <param name="pk"></param>
        /// <returns></returns>
        public static Int32 GetLength(Packet pk)
        {
            // 4+(Version)1+(Kind)1+(seq)2+(sim)6+(channel)1+(DataType)1
            if (pk.Total < 16 + 2) return 0;

            var dataType = (DataTypes)(pk[15] >> 4);

            // 头部长度
            var len = 4 + 1 + 1 + 2 + 6 + 1 + 1;
            if (dataType != DataTypes.透传数据) len += 8;
            if (dataType <= DataTypes.视频B帧) len += 2 + 2;

            var pk2 = pk.Slice(len);
            if (pk2.Total < 2) return 0;

            len += pk2.ReadUInt16(false);

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