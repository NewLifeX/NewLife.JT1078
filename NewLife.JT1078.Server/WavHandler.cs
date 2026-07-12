using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>WAV 音频格式处理器。将音频帧解码为 PCM 并添加 WAV 头后推送</summary>
public class WavHandler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    private Boolean _headerWritten;
    #endregion

    #region 方法
    /// <summary>编码 WAV 头</summary>
    private static Byte[] CreateWavHeader(Int32 dataSize, Int32 sampleRate = 8000, Int16 channels = 1, Int16 bitsPerSample = 16)
    {
        using var ms = new MemoryStream(44);
        using var writer = new BinaryWriter(ms);

        writer.Write(new[] { (Byte)0x52, (Byte)0x49, (Byte)0x46, (Byte)0x46 }); // RIFF
        writer.Write(36 + dataSize); // 文件大小
        writer.Write(new[] { (Byte)0x57, (Byte)0x41, (Byte)0x56, (Byte)0x45 }); // WAVE
        writer.Write(new[] { (Byte)0x66, (Byte)0x6D, (Byte)0x74, (Byte)0x20 }); // fmt
        writer.Write(16); // fmt 块大小
        writer.Write((Int16)1); // PCM 格式
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8); // 字节率
        writer.Write((Int16)(channels * bitsPerSample / 8)); // 块对齐
        writer.Write(bitsPerSample);
        writer.Write(new[] { (Byte)0x64, (Byte)0x61, (Byte)0x74, (Byte)0x61 }); // data
        writer.Write(dataSize);

        return ms.ToArray();
    }

    /// <summary>编码消息并推送</summary>
    public async Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.AudioFrame) return 0;

        users = users?.Where(e => e.Type == RTPType.Wav).ToArray();
        if (users == null || users.Length == 0) return 0;

        using var span = DefaultTracer.Instance?.NewSpan("WavHandler.Encode", msg);

        var data = msg.Payload?.ReadBytes();
        if (data == null || data.Length == 0) return 0;

        // 解码为 PCM
        var pcm = Hub.AudioCodec.ToPcm(msg.Type, data);
        if (pcm == null || pcm.Length == 0) return 0;

        foreach (var session in users)
        {
            // 首次推送 WAV 头
            if (!_headerWritten)
            {
                var header = CreateWavHeader(pcm.Length);
                await session.WriteAsync(header).ConfigureAwait(false);
                _headerWritten = true;
            }

            await session.WriteAsync(pcm).ConfigureAwait(false);
            session.LastActive = DateTime.Now;
        }

        return users.Length;
    }
    #endregion
}
