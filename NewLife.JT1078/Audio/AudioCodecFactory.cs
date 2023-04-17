using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Audio
{
    public class AudioCodecFactory
    {
        private readonly ADPCMCodec adpcmCodec = new();
        private readonly G711ACodec g711ACodec = new();
        private readonly G711UCodec g711UCodec = new();
        private Boolean _hasHI;

        // 海思芯片编码的音频需要移除海思头，可能还有其他的海思头
        //private static readonly Byte[] HI = new Byte[] { 0x00, 0x01, 0x52, 0x00 };
        private static readonly Byte[] HI = new Byte[] { 0x00, 0x01 };

        /// <summary>去除海思头</summary>
        /// <param name="data">设备数据</param>
        /// <param name="trim">是否已去除海思头</param>
        /// <returns></returns>
        public ReadOnlySpan<Byte> TrimHI(ReadOnlySpan<Byte> data, out Boolean trim)
        {
            if (data.StartsWith(HI) && data[3] == 0x00 && data[2] == (Byte)((data.Length - 4) / 2))
            {
                data = data[4..];
                trim = true;
            }
            else
            {
                trim = false;
            }

            return data;
        }

        /// <summary>添加海思头</summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public Byte[] AddHI(Byte[] data)
        {
            var buf = new Byte[4 + data.Length];
            buf[0] = 0x00;
            buf[1] = 0x01;
            buf[2] = (Byte)(data.Length / 2);
            buf[3] = 0x00;

            data.CopyTo(buf, 4);

            return buf;
        }

        /// <summary>设备数据转PCM编码</summary>
        /// <param name="avType"></param>
        /// <param name="bodies"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public Byte[] ToPcm(AVTypes avType, Byte[] bodies)
        {
            var data = TrimHI(bodies, out _hasHI);

            return avType switch
            {
                AVTypes.ADPCMA => adpcmCodec.ToPcm(data, null),
                AVTypes.G711A => g711ACodec.ToPcm(data, null),
                AVTypes.G711U => g711UCodec.ToPcm(data, null),
                _ => throw new NotSupportedException($"[{avType}] NotSupported"),
            };
        }

        /// <summary>PCM编码转设备数据</summary>
        /// <param name="avType"></param>
        /// <param name="bodies"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public Byte[] FromPcm(AVTypes avType, Byte[] bodies)
        {
            var rs = avType switch
            {
                AVTypes.ADPCMA => adpcmCodec.FromPcm(bodies, null),
                AVTypes.G711A => g711ACodec.FromPcm(bodies, null),
                AVTypes.G711U => g711UCodec.FromPcm(bodies, null),
                _ => throw new NotSupportedException($"[{avType}] NotSupported"),
            };

            // 添加海思头
            if (_hasHI) rs = AddHI(rs);

            return rs;
        }
    }
}