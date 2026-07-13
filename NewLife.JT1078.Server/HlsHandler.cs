using NewLife.Audio;
using NewLife.Audio.AAC;
using NewLife.Data;
using NewLife.JT1078.H264;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server.Hls;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>HLS 直播格式处理器。将音视频帧切片为 TS 分片并生成 M3U8 播放列表</summary>
/// <remarks>
/// 使用纯 C# MPEG-TS 复用器将 H.264 视频和 AAC 音频封装为 TS 段，
/// 通过 HlsSegmentCache 存储并提供给 HTTP 端点。
/// </remarks>
public class HlsHandler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    /// <summary>HLS 段缓存</summary>
    public HlsSegmentCache SegmentCache { get; set; }

    private TSMuxer _muxer;
    private readonly H264Decoder _h264Decoder = new();
    private FaacEncoder _faacEncoder;
    private DateTime _segmentStart;
    private Int64 _pts;
    private Int64 _lastVideoPts;
    private Boolean _hasVideo;
    private Int32 _frameCount;

    /// <summary>段时长（秒）</summary>
    public Double SegmentDuration { get; set; } = 2.0;
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.VideoIFrame && msg.DataType != DataTypes.VideoPFrame &&
            msg.DataType != DataTypes.VideoBFrame && msg.DataType != DataTypes.AudioFrame)
            return Task.FromResult(0);

        users = users?.Where(e => e.Type == RTPType.Hls).ToArray();
        if (users == null || users.Length == 0) return Task.FromResult(0);

        try
        {
            ProcessMessage(msg);
        }
        catch (Exception ex)
        {
            WriteLog("EncodeAsync 错误: {0}", ex.Message);
        }

        return Task.FromResult(users.Length);
    }

    private void ProcessMessage(JTMessage msg)
    {
        // 使用默认静态实例（可从 DI 覆盖）
        SegmentCache ??= HlsSegmentCache.Default;

        var key = HlsSegmentCache.GetKey(Hub.SIM, Hub.Channel);

        // 初始化复用器
        if (_muxer == null)
        {
            _muxer = new TSMuxer();
            _segmentStart = DateTime.Now;
            _pts = 0;
            _frameCount = 0;
            _hasVideo = false;

            // 检测是否有音频编码器可用（FaacEncoder 需要 libfaac.dll）
            try
            {
                _faacEncoder = new FaacEncoder(8000, 1, 16, true);
                _muxer.HasAudio = true;
            }
            catch
            {
                _muxer.HasAudio = false;
            }

            _muxer.Reset();
        }

        // 处理视频帧
        if (msg.DataType == DataTypes.VideoIFrame || msg.DataType == DataTypes.VideoPFrame ||
            msg.DataType == DataTypes.VideoBFrame)
        {
            ProcessVideo(msg);
        }
        // 处理音频帧
        else if (msg.DataType == DataTypes.AudioFrame)
        {
            ProcessAudio(msg);
        }

        // 检查是否需要分段
        var elapsed = (DateTime.Now - _segmentStart).TotalSeconds;
        if (elapsed >= SegmentDuration && _hasVideo)
        {
            FinalizeSegment(key);
        }
    }

    private void ProcessVideo(JTMessage msg)
    {
        var buf = msg.Payload;
        if (buf == null || buf.Total == 0) return;

        // 读取 payload 数据
        var rawData = new Byte[buf.Total];
        for (var i = 0; i < rawData.Length; i++) rawData[i] = buf[i];

        // 创建 JTMessage 包装用于 H264Decoder
        var decodeMsg = new JTMessage
        {
            Payload = new NewLife.Data.ArrayPacket(rawData),
            Type = msg.Type,
            DataType = msg.DataType,
        };

        // 解析 NAL 单元
        var nalus = _h264Decoder.ParseNALU(decodeMsg);
        if (nalus == null || nalus.Count == 0) return;

        _hasVideo = true;

        // 累加 PTS（90kHz 时钟，假设 25fps）
        if (_lastVideoPts == 0)
        {
            _lastVideoPts = _pts;
        }
        else
        {
            _lastVideoPts += 3600; // 90000 / 25 = 3600 per frame
        }
        _pts = _lastVideoPts;

        // 将 NAL 数据转换为长度前缀格式（用于 TS）
        var nalData = NalUnitToLengthPrefixed(rawData, nalus);

        if (nalData.Length > 0)
        {
            _muxer.WriteVideo(nalData, _pts);
        }

        _frameCount++;
    }

    private void ProcessAudio(JTMessage msg)
    {
        if (_faacEncoder == null) return;

        var buf = msg.Payload;
        if (buf == null || buf.Total == 0) return;

        // 解码为 PCM
        var pcm = Hub.AudioCodec.ToPcm((NewLife.Audio.AVTypes)msg.Type, buf);
        if (pcm == null || pcm.Total == 0) return;

        var pcmBytes = new Byte[pcm.Total];
        for (var i = 0; i < pcmBytes.Length; i++) pcmBytes[i] = pcm[i];

        // 逐帧编码 AAC
        var step = _faacEncoder.FrameSize;
        var offset = 0;
        while (offset < pcmBytes.Length)
        {
            var len = Math.Min(step, pcmBytes.Length - offset);
            var input = new Byte[len];
            Buffer.BlockCopy(pcmBytes, offset, input, 0, len);

            var output = new Byte[_faacEncoder.MaxOutput];
            var encoded = _faacEncoder.Encode(input, output);
            if (encoded > 0)
            {
                var aacFrame = new Byte[encoded];
                Buffer.BlockCopy(output, 0, aacFrame, 0, encoded);

                // AAC PTS = approximate based on time
                var audioPts = _pts;
                _muxer.WriteAudio(aacFrame, audioPts);
            }

            offset += len;
        }
    }

    /// <summary>将原始 H.264 数据中的 NAL 单元转换为长度前缀格式</summary>
    private static Byte[] NalUnitToLengthPrefixed(ReadOnlySpan<Byte> rawData, List<H264NALU> nalus)
    {
        using var ms = new MemoryStream(rawData.Length);
        foreach (var nalu in nalus)
        {
            if (nalu.RawData == null || nalu.RawData.Length == 0) continue;

            // 4 字节大端长度前缀
            var len = nalu.RawData.Length;
            ms.WriteByte((Byte)((len >> 24) & 0xFF));
            ms.WriteByte((Byte)((len >> 16) & 0xFF));
            ms.WriteByte((Byte)((len >> 8) & 0xFF));
            ms.WriteByte((Byte)(len & 0xFF));

            ms.Write(nalu.RawData);
        }
        return ms.ToArray();
    }

    private void FinalizeSegment(String key)
    {
        if (!_hasVideo) return;

        var data = _muxer.FinalizeSegment();
        if (data.Length > 0)
        {
            var duration = (DateTime.Now - _segmentStart).TotalSeconds;
            SegmentCache.AddSegment(key, data, duration);
            WriteLog("HLS 段完成 seq={0} size={1}KB duration={2:F2}s",
                SegmentCache.GetCurrentSequence(key), data.Length / 1024, duration);
        }

        // 重置并开始新段
        _muxer.Reset();
        _segmentStart = DateTime.Now;
        _hasVideo = false;
        _lastVideoPts = 0;
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _muxer.TryDispose();
        _faacEncoder.TryDispose();
    }
    #endregion

    #region 辅助
    private void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.Info("HlsHandler." + String.Format(format, args));
    }
    #endregion
}
