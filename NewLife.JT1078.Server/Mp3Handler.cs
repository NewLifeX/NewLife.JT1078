using NewLife.Audio;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server.Mp3;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>MP3 音频格式处理器。将音频帧解码为 PCM 后编码为 MP3</summary>
/// <remarks>
/// 使用纯 C# 简易 MP3 编码器（Mp3Encoder），将 G.711/ADPCM 解码后的 PCM 编码为 MPEG-2.5 Layer III 帧。
/// 支持 8kHz 单声道语音编码，固定比特率 64kbps。
/// 不依赖外部原生库，纯 .NET 实现。
/// </remarks>
public class Mp3Handler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    private Mp3Encoder _mp3Encoder;
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public async Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.AudioFrame) return 0;

        users = users?.Where(e => e.Type == RTPType.Mp3).ToArray();
        if (users == null || users.Length == 0) return 0;

        using var span = DefaultTracer.Instance?.NewSpan("Mp3Handler.Encode", msg);

        var buf = msg.Payload;
        if (buf == null || buf.Total == 0) return 0;

        // 解码为 PCM
        var pcm = Hub.AudioCodec.ToPcm((NewLife.Audio.AVTypes)msg.Type, buf);
        if (pcm == null || pcm.Total == 0) return 0;

        _mp3Encoder ??= new Mp3Encoder(8000, 1, 64000);

        var pcmBytes = new Byte[pcm.Total];
        for (var i = 0; i < pcmBytes.Length; i++) pcmBytes[i] = pcm[i];

        // 编码为 MP3 帧
        var maxOutput = _mp3Encoder.MaxOutput;
        var output = new Byte[maxOutput];
        var encoded = _mp3Encoder.Encode(pcmBytes, output);

        if (encoded > 0)
        {
            var mp3Data = new Byte[encoded];
            Buffer.BlockCopy(output, 0, mp3Data, 0, encoded);

            foreach (var session in users)
            {
                await session.WriteAsync(mp3Data).ConfigureAwait(false);
                session.LastActive = DateTime.Now;
            }
        }

        return users.Length;
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _mp3Encoder.TryDispose();
        _mp3Encoder = null;
    }
    #endregion
}
