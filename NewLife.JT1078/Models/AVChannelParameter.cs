using NewLife.Serialization;

namespace NewLife.JT1078.Models
{
    /// <summary>单独通道视频参数设置</summary>
    public class AVChannelParameter : IAccessor
    {
        #region 属性
        /// <summary>需单独设置视频参数的通道数量</summary>
        public Byte Count { get; set; }

        /// <summary></summary>
        [FieldSize(nameof(Count))]
        public AVChannelParameterItem[] Items { get; set; }
        #endregion

        public Boolean Read(Stream stream, Object context)
        {
            var reader = new Binary { Stream = stream, UseRef = false };

            Count = reader.ReadByte();

            var list = new List<AVChannelParameterItem>();
            for (var i = 0; i < Count; i++)
            {
                if (stream.Position + 21 > stream.Length) break;

                // AVParameter 有21个字节，最后一个字节在这用不到。这里给1个字节逻辑通道号，20个字节用于AVParameter
                var buf = reader.ReadBytes(21);
                list.Add(Binary.FastRead<AVChannelParameterItem>(new MemoryStream(buf)));
            }
            Items = list.ToArray();

            return true;
        }

        public Boolean Write(Stream stream, Object context)
        {
            Count = (Byte)(Items?.Length ?? 0);

            var writer = new Binary { Stream = stream, UseRef = false };
            writer.WriteByte(Count);
            for (var i = 0; i < Count; i++)
            {
                writer.Write(Items[i]);
                // 末尾1字节为AVParameter未使用字节，回退覆盖，避免出现在报文中
                stream.Seek(-1, SeekOrigin.Current);
            }

            // 最后一个条目的未使用字节同样回收
            if (stream.CanSeek) stream.SetLength(stream.Position);

            return true;
        }
    }

    /// <summary>通道参数项。包含逻辑通道号及对应音视频参数</summary>
    public class AVChannelParameterItem
    {
        /// <summary>逻辑通道号</summary>
        public Byte ChennalId { get; set; }

        /// <summary>音视频参数</summary>
        public AVParameter Pasrameter { get; set; }
    }
}