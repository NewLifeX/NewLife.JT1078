using NewLife.JT1078.Audio;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>AAC 音频格式处理器。将音频帧解码为 PCM 后再编码为 AAC</summary>
public class AacHandler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    private FaacEncoder _faacEncoder;
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public async Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.AudioFrame) return 0;

        users = users?.Where(e => e.Type == RTPType.Aac).ToArray();
        if (users == null || users.Length == 0) return 0;

        using var span = DefaultTracer.Instance?.NewSpan("AacHandler.Encode", msg);

        var data = msg.Payload?.ReadBytes();
        if (data == null || data.Length == 0) return 0;

        // 解码为 PCM
        var pcm = Hub.AudioCodec.ToPcm(msg.Type, data);
        if (pcm == null || pcm.Length == 0) return 0;

        _faacEncoder ??= new FaacEncoder(8000, 1, 16, true);

        // 逐帧编码 AAC
        var step = _faacEncoder.FrameSize;
        var offset = 0;
        while (offset < pcm.Length)
        {
            var len = Math.Min(step, pcm.Length - offset);
            var input = new Byte[len];
            Buffer.BlockCopy(pcm, offset, input, 0, len);

            var output = new Byte[_faacEncoder.MaxOutput];
            var encoded = _faacEncoder.Encode(input, output);
            if (encoded > 0)
            {
                var trimmed = new Byte[encoded];
                Buffer.BlockCopy(output, 0, trimmed, 0, encoded);
                foreach (var session in users)
                {
                    await session.WriteAsync(trimmed).ConfigureAwait(false);
                    session.LastActive = DateTime.Now;
                }
            }

            offset += len;
        }

        return users.Length;
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        _faacEncoder.TryDispose();
        _faacEncoder = null;
    }
    #endregion
}
