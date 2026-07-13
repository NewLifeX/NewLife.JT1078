namespace NewLife.JT1078.Server.Mp3;

/// <summary>纯 C# 简易 MP3 编码器（MPEG-2.5 Layer III，8kHz 单声道）</summary>
/// <remarks>
/// 使用 MDCT + 量化将 PCM 16-bit 编码为 MP3 帧。
/// 支持 8kHz 单声道输入，固定比特率 64kbps。
/// 适用于 G.711/ADPCM 解码后的语音编码。
/// </remarks>
public class Mp3Encoder : IDisposable
{
    #region 属性
    /// <summary>采样率</summary>
    public Int32 SampleRate { get; }

    /// <summary>声道数</summary>
    public Int32 Channels { get; }

    /// <summary>比特率</summary>
    public Int32 Bitrate { get; }

    /// <summary>每帧采样数</summary>
    public Int32 FrameSize { get; } = 576;

    /// <summary>最大输出字节数</summary>
    public Int32 MaxOutput { get; }

    /// <summary>内部待处理缓冲区的 PCM 样本数</summary>
    private readonly Double[] _pcmBuffer;
    private Int32 _pcmCount;
    private Boolean _disposed;
    #endregion

    /// <summary>实例化 MP3 编码器</summary>
    /// <param name="sampleRate">采样率（Hz），仅支持 8000</param>
    /// <param name="channels">声道数，仅支持 1（单声道）</param>
    /// <param name="bitrate">比特率（bps），仅支持 64000</param>
    public Mp3Encoder(Int32 sampleRate = 8000, Int32 channels = 1, Int32 bitrate = 64000)
    {
        SampleRate = sampleRate;
        Channels = channels;
        Bitrate = bitrate;

        // MPEG-2.5 Layer III 帧大小 = 576 样本
        // 每帧输出大小 = 帧头(4) + 侧信息(17) + 主数据
        // 主数据大小 = bitrate * 1152 / samplerate - header(4) - side(17)
        // 对于 8kHz 576 样本帧，帧时长 = 576/8000 = 0.072 秒
        // 但 MP3 帧头中的比特率是按 1152 样本帧计算的，需要调整
        // 对于 MPEG-2.5，帧大小 = (144 * bitrate / samplerate) + padding
        // 使用 MPEG 帧大小公式: frame_size = (144 * bitrate_kbps * 1000 / samplerate) + padding
        // 对于 64kbps, 8kHz: (144 * 64 * 1000 / 8000) = 1152 bytes
        // 但这是针对 1152 样本帧，我们使用 576 样本帧，所以除 2
        
        _pcmBuffer = new Double[FrameSize];
        _pcmCount = 0;
        MaxOutput = (Int32)((144.0 * (bitrate / 1000.0) * 1000.0 / sampleRate) / 2 + 100);
        if (MaxOutput < 200) MaxOutput = 200;
    }

    /// <summary>编码 PCM 数据到 MP3</summary>
    /// <param name="pcm">PCM 16-bit 单声道数据（每个样本 2 字节，小端）</param>
    /// <param name="output">输出缓冲区</param>
    /// <returns>编码字节数，0 表示需要更多数据</returns>
    public Int32 Encode(Byte[] pcm, Byte[] output)
    {
        if (_disposed) return 0;

        var sampleCount = pcm.Length / 2; // 16-bit samples
        var offset = 0;
        var totalEncoded = 0;

        while (offset < sampleCount)
        {
            // 填充内部缓冲区
            var needed = FrameSize - _pcmCount;
            var available = sampleCount - offset;
            var copyCount = Math.Min(needed, available);

            for (var i = 0; i < copyCount; i++)
            {
                var lo = pcm[(offset + i) * 2];
                var hi = pcm[(offset + i) * 2 + 1];
                var sample = (Int16)(hi << 8 | lo);
                _pcmBuffer[_pcmCount + i] = sample / 32768.0;
            }

            _pcmCount += copyCount;
            offset += copyCount;

            // 缓冲区满时编码一帧
            if (_pcmCount >= FrameSize)
            {
                var encoded = EncodeFrame(output, totalEncoded);
                totalEncoded += encoded;
                _pcmCount = 0;
            }
        }

        return totalEncoded;
    }

    /// <summary>刷新剩余数据（填充零）</summary>
    public Byte[] Flush()
    {
        if (_pcmCount > 0)
        {
            // 用零填充剩余
            while (_pcmCount < FrameSize)
            {
                _pcmBuffer[_pcmCount++] = 0;
            }

            var output = new Byte[MaxOutput];
            var len = EncodeFrame(output, 0);
            _pcmCount = 0;

            if (len > 0)
            {
                var result = new Byte[len];
                Buffer.BlockCopy(output, 0, result, 0, len);
                return result;
            }
        }

        return [];
    }

    /// <summary>编码一帧（576 个 PCM 样本 → MP3 帧）</summary>
    private Int32 EncodeFrame(Byte[] output, Int32 offset)
    {
        if (output.Length - offset < MaxOutput) return 0;

        // 1. 应用窗口（正弦窗）
        var windowed = new Double[FrameSize];
        for (var i = 0; i < FrameSize; i++)
        {
            // 正弦窗: sin(pi * (i + 0.5) / N)
            var w = Math.Sin(Math.PI * (i + 0.5) / FrameSize);
            windowed[i] = _pcmBuffer[i] * w;
        }

        // 2. MDCT（简化为 DCT-IV）
        var mdctOut = new Double[FrameSize];
        for (var k = 0; k < FrameSize; k++)
        {
            var sum = 0.0;
            for (var i = 0; i < FrameSize; i++)
            {
                sum += windowed[i] * Math.Cos(Math.PI / FrameSize * (i + 0.5) * (k + 0.5));
            }
            mdctOut[k] = sum;
        }

        // 3. 简化的心理声学模型：计算每个子带的能量
        var subbands = 32;
        var bandSize = FrameSize / subbands; // 18

        // 4. 量化（简单均匀量化）
        var quantized = new Int32[FrameSize];
        var maxCoeff = 0.0;
        for (var i = 0; i < FrameSize; i++)
        {
            if (Math.Abs(mdctOut[i]) > maxCoeff) maxCoeff = Math.Abs(mdctOut[i]);
        }

        // 全局增益：使最大系数映射到合理的量化范围
        var globalGain = 210; // 起始增益
        var scalefac = maxCoeff > 0.001 ? 8191.0 / maxCoeff : 1.0;

        // 对每个子带应用不同的缩放因子（简化）
        for (var b = 0; b < subbands; b++)
        {
            var start = b * bandSize;
            var end = Math.Min(start + bandSize, FrameSize);

            // 计算子带能量
            var energy = 0.0;
            for (var i = start; i < end; i++)
            {
                energy += mdctOut[i] * mdctOut[i];
            }
            energy = Math.Sqrt(energy / (end - start));

            // 根据能量调整缩放（较高能量 = 较多比特）
            var bandScale = scalefac * (1.0 + energy * 0.5);

            for (var i = start; i < end; i++)
            {
                quantized[i] = (Int32)Math.Round(mdctOut[i] * bandScale);
                if (quantized[i] > 8191) quantized[i] = 8191;
                if (quantized[i] < -8191) quantized[i] = -8191;
            }
        }

        // 5. 计算帧大小（字节）
        // 对于 MPEG-2.5 Layer III，576 样本帧
        // 帧大小 = (144 * bitrate_kbps / samplerate_khz) 用于 1152 样本帧
        // 对于 576 样本帧，减半
        var frameSize = CalculateFrameSize(SampleRate, Bitrate, 0);

        // 6. 写入帧头
        var pos = WriteHeader(output, offset, frameSize);

        // 7. 写入侧信息（MPEG-2.5，单声道 = 17 字节）
        pos += WriteSideInfo(output, offset + pos, globalGain);

        // 8. 写入主数据（量化后的 MDCT 系数）
        pos += WriteMainData(output, offset + pos, quantized, frameSize - (pos));

        return pos;
    }

    /// <summary>计算 MP3 帧大小</summary>
    private static Int32 CalculateFrameSize(Int32 sampleRate, Int32 bitrate, Int32 padding)
    {
        // MPEG-1: frame_size = (144 * bitrate_kbps * 1000 / samplerate) + padding
        // MPEG-2/2.5: 样本数为一半，帧大小也减半
        // 但帧头中的比特率字段是按 MPEG-1 帧（1152 样本）定义的
        // 对于 MPEG-2.5，实际帧大小 = (144 * bitrate_kbps * 1000 / samplerate) / 2 + padding
        var bitrateKbps = bitrate / 1000;
        var size = (Int32)(144.0 * bitrateKbps * 1000.0 / sampleRate / 2.0 + padding);
        return Math.Max(size, 100);
    }

    /// <summary>写入 MP3 帧头（4 字节）</summary>
    private static Int32 WriteHeader(Byte[] output, Int32 offset, Int32 frameSize)
    {
        // MPEG-2.5 Layer III, 单声道, 8kHz, 64kbps, 无 CRC
        // sync word: 12 bits (0xFFF)
        // ID: 1 bit (0 = MPEG-2.5, 1 = MPEG-1)
        // Layer: 2 bits (01 = Layer III)
        // protection: 1 bit (1 = no CRC)
        // bitrate_index: 4 bits
        // sampling_frequency: 2 bits
        // padding: 1 bit
        // private: 1 bit
        // channel_mode: 2 bits (11 = mono)
        // mode_extension: 2 bits (00)
        // copyright: 1 bit
        // original: 1 bit
        // emphasis: 2 bits (00 = none)

        var bitrateIndex = BitrateIndex(64000, 0); // MPEG-2.5
        var sampRateIndex = SampleRateIndex(8000);
        var padding = 0; // 无填充

        var hdr = 0xFFF00000u; // sync word
        hdr |= 0u << 19; // ID = 0 (MPEG-2.5)
        hdr |= 1u << 17; // Layer = 01 (Layer III)
        hdr |= 1u << 16; // protection = 1 (no CRC)
        hdr |= (UInt32)bitrateIndex << 12;
        hdr |= (UInt32)sampRateIndex << 10;
        hdr |= (UInt32)padding << 9;
        hdr |= 0u << 8; // private = 0
        hdr |= 3u << 6; // channel_mode = 11 (mono)
        hdr |= 0u << 4; // mode_extension = 00
        hdr |= 0u << 3; // copyright = 0
        hdr |= 1u << 2; // original = 1
        hdr |= 0u;      // emphasis = 00

        output[offset] = (Byte)((hdr >> 24) & 0xFF);
        output[offset + 1] = (Byte)((hdr >> 16) & 0xFF);
        output[offset + 2] = (Byte)((hdr >> 8) & 0xFF);
        output[offset + 3] = (Byte)(hdr & 0xFF);

        return 4;
    }

    /// <summary>写入侧信息（MPEG-2.5 单声道 = 17 字节）</summary>
    private static Int32 WriteSideInfo(Byte[] output, Int32 offset, Int32 globalGain)
    {
        // MPEG-2.5 Layer III 单声道侧信息结构：
        // main_data_begin: 9 bits
        // private_bits: 5 bits
        // scfsi: 4 bits (2 bands × 2 scalefactor select info)
        // 每声道/颗粒:
        //   part2_3_length: 12 bits
        //   big_values: 9 bits
        //   global_gain: 8 bits
        //   scalefac_compress: 9 bits
        //   window_switching: 1 bit (0 = normal)
        //   如果是 normal:
        //     block_type: 2 bits (0 = normal)
        //     mixed_block_flag: 1 bit
        //   table_select: 5 bits × 3 = 15 bits
        //   subblock_gain: 3 bits × 3 = 9 bits
        //   region0_count: 4 bits
        //   region1_count: 4 bits
        //   preflag: 1 bit
        //   scalefac_scale: 1 bit
        //   count1table_select: 1 bit

        var pos = 0;

        // main_data_begin = 0（无比特池）
        WriteBits(output, offset, ref pos, 9, 0);
        // private_bits = 0
        WriteBits(output, offset, ref pos, 5, 0);
        // scfsi = 0 (全部使用第 2 组缩放因子)
        WriteBits(output, offset, ref pos, 4, 0);

        // 单声道，单颗粒
        // part2_3_length = 主要数据长度（简化：使用剩余的帧空间）
        var mainDataBits = (output.Length - offset - pos / 8 - 4) * 8; // 减 4 字节预留
        if (mainDataBits > 4000) mainDataBits = 4000;
        WriteBits(output, offset, ref pos, 12, mainDataBits);
        // big_values = 最大量化值数 / 2
        WriteBits(output, offset, ref pos, 9, 288);
        // global_gain = 8 bits (0-255)
        WriteBits(output, offset, ref pos, 8, Math.Min(globalGain, 255));
        // scalefac_compress = 0 (简单均匀量化)
        WriteBits(output, offset, ref pos, 9, 0);
        // window_switching = 0 (正常窗口)
        WriteBits(output, offset, ref pos, 1, 0);
        // block_type = 0 (normal)
        WriteBits(output, offset, ref pos, 2, 0);
        // mixed_block_flag = 0
        WriteBits(output, offset, ref pos, 1, 0);

        // table_select (3 × 5 bits)
        WriteBits(output, offset, ref pos, 5, 0); // 使用 Huffman 表 0（无编码）
        WriteBits(output, offset, ref pos, 5, 0);
        WriteBits(output, offset, ref pos, 5, 0);

        // subblock_gain (3 × 3 bits)
        WriteBits(output, offset, ref pos, 3, 0);
        WriteBits(output, offset, ref pos, 3, 0);
        WriteBits(output, offset, ref pos, 3, 0);

        // region0_count
        WriteBits(output, offset, ref pos, 4, 7);
        // region1_count
        WriteBits(output, offset, ref pos, 4, 20);
        // preflag = 0
        WriteBits(output, offset, ref pos, 1, 0);
        // scalefac_scale = 0
        WriteBits(output, offset, ref pos, 1, 0);
        // count1table_select = 0
        WriteBits(output, offset, ref pos, 1, 0);

        // 对齐到字节边界
        while (pos % 8 != 0)
        {
            WriteBits(output, offset, ref pos, 1, 0);
        }

        return pos / 8;
    }

    /// <summary>写入主数据（量化系数）</summary>
    private static Int32 WriteMainData(Byte[] output, Int32 offset, Int32[] quantized, Int32 maxBytes)
    {
        var pos = 0;
        var maxBits = maxBytes * 8;

        // 使用简单的 run-length 编码写入量化系数
        // 大值区域（big_values）：每 2 个系数为一组
        var bigValues = quantized.Length / 2;

        for (var i = 0; i < bigValues && pos < maxBits; i++)
        {
            var x = quantized[i * 2];
            var y = quantized[i * 2 + 1];

            // 限幅
            if (x > 15) x = 15;
            if (x < -15) x = -15;
            if (y > 15) y = 15;
            if (y < -15) y = -15;

            // 简单的 4+4 位编码 (unsigned magnitude)
            var absX = Math.Abs(x);
            var absY = Math.Abs(y);
            var signX = x < 0 ? 1 : 0;
            var signY = y < 0 ? 1 : 0;

            // 使用 Huffman 表 1 的简化版本：线性编码
            // 大值 quad: 每对系数编码为 signX(1) + absX(4) + signY(1) + absY(4) = 10 bits
            if (absX > 0 || absY > 0)
            {
                if (pos + 10 <= maxBits)
                {
                    WriteBits(output, offset, ref pos, 1, signX);
                    WriteBits(output, offset, ref pos, 4, absX);
                    WriteBits(output, offset, ref pos, 1, signY);
                    WriteBits(output, offset, ref pos, 4, absY);
                }
            }
            else
            {
                // count1: 四个线性系数 (2 bits per sample = 4 bits per quad)
                if (pos + 4 <= maxBits)
                {
                    WriteBits(output, offset, ref pos, 4, 0);
                }
            }
        }

        // 填充剩余位为 0
        while (pos < maxBits)
        {
            WriteBits(output, offset, ref pos, 1, 0);
            if (pos >= maxBits) break;
        }

        // 对齐到字节
        while (pos % 8 != 0)
        {
            WriteBits(output, offset, ref pos, 1, 0);
        }

        return pos / 8;
    }

    /// <summary>向字节数组写入指定位数</summary>
    private static void WriteBits(Byte[] output, Int32 offset, ref Int32 pos, Int32 count, Int32 value)
    {
        for (var i = count - 1; i >= 0; i--)
        {
            var bit = (value >> i) & 1;
            var bytePos = pos / 8;
            var bitPos = 7 - (pos % 8);

            if (offset + bytePos < output.Length)
            {
                if (bit != 0)
                    output[offset + bytePos] |= (Byte)(1 << bitPos);
                else
                    output[offset + bytePos] &= (Byte)~(1 << bitPos);
            }

            pos++;
        }
    }

    /// <summary>获取比特率索引</summary>
    private static Int32 BitrateIndex(Int32 bitrate, Int32 version)
    {
        // MPEG-2.5 bitrate index (kbps)
        // Index: 1=8, 2=16, 3=24, 4=32, 5=40, 6=48, 7=56, 8=64, 9=80, 10=96, 11=112, 12=128, 13=144, 14=160
        return bitrate switch
        {
            8000 => 1,
            16000 => 2,
            24000 => 3,
            32000 => 4,
            40000 => 5,
            48000 => 6,
            56000 => 7,
            64000 => 8,
            80000 => 9,
            96000 => 10,
            112000 => 11,
            128000 => 12,
            144000 => 13,
            160000 => 14,
            _ => 8, // 默认 64kbps
        };
    }

    /// <summary>获取采样率索引</summary>
    private static Int32 SampleRateIndex(Int32 sampleRate)
    {
        // MPEG-2.5: 0=8kHz, 1=11.025kHz, 2=12kHz
        return sampleRate switch
        {
            8000 => 0,
            11025 => 1,
            12000 => 2,
            _ => 0,
        };
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _disposed = true;
    }
}
