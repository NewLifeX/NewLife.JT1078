using NewLife;
using NewLife.Data;
using NewLife.Messaging;
using NewLife.Model;
using NewLife.Net;

namespace NewLife.JT1078.Protocols
{
    /// <summary>JT808网络封包</summary>
    public class JTCodec : Handler
    {
        const String CodecName = nameof(JTCodec);
        private Int32 _gid;

        /// <summary>写入数据</summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override Object Write(IHandlerContext context, Object message)
        {
            if (message is JTMessage msg)
            {
                if (msg.Sequence == 0) msg.Sequence = (Byte)Interlocked.Increment(ref _gid);

                message = msg.ToPacket();
            }

            return base.Write(context, message);
        }

        /// <summary>读取数据</summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public override Object Read(IHandlerContext context, Object message)
        {
            if (message is not IPacket pk) return base.Read(context, message);

            // 解码得到多个消息
            var list = Decode(context, pk);
            if (list == null) return null;

            foreach (var msg in list)
                // 匹配输入回调，让上层事件收到分包信息。
                // 这里很可能处于网络IO线程，阻塞了下一个Tcp包的接收
                base.Read(context, msg);

            return null;
        }

        /// <summary>连接关闭时，清空粘包编码器</summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public override Boolean Close(IHandlerContext context, String reason)
        {
            if (context.Owner is IExtend ss) ss[CodecName] = null;

            return base.Close(context, reason);
        }

        #region 粘包处理
        /// <summary>解码</summary>
        /// <param name="context"></param>
        /// <param name="pk">包</param>
        /// <returns></returns>
        protected IList<JTMessage> Decode(IHandlerContext context, IPacket pk)
        {
            var ss = context.Owner as IExtend;
            if (ss[CodecName] is not PacketCodec pc)
                ss[CodecName] = pc = new PacketCodec
                {
                    MaxCache = 1024,
#pragma warning disable CS0618 // PacketCodec.GetLength 已弃用，但 GetLength2 签名未知
                    GetLength = JTMessage.GetLength,
#pragma warning restore CS0618
                    Tracer = (context.Owner as ISocket)?.Tracer
                };

            var pks = pc.Parse(pk);
            var list = pks.Select(e =>
            {
                var msg = new JTMessage();
                if (!msg.Read(e)) return null;

                return msg;
            }).ToList();

            return list;
        }
        #endregion
    }
}