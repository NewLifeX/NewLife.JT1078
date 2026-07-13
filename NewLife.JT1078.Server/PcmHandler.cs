using NewLife.Audio;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>PCM 音频格式处理器。将音频帧解码为原始 PCM 并推送</summary>
public class PcmHandler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public async Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.AudioFrame) return 0;

        users = users?.Where(e => e.Type == RTPType.Pcm).ToArray();
        if (users == null || users.Length == 0) return 0;

        using var span = DefaultTracer.Instance?.NewSpan("PcmHandler.Encode", msg);

        var buf = msg.Payload;
        if (buf == null || buf.Total == 0) return 0;

        // 解码为 PCM
        var pcm = Hub.AudioCodec.ToPcm((NewLife.Audio.AVTypes)msg.Type, buf);
        if (pcm == null || pcm.Total == 0) return 0;

        var pcmBytes = new Byte[pcm.Total];
        for (var i = 0; i < pcmBytes.Length; i++) pcmBytes[i] = pcm[i];

        foreach (var session in users)
        {
            await session.WriteAsync(pcmBytes).ConfigureAwait(false);
            session.LastActive = DateTime.Now;
        }

        return users.Length;
    }
    #endregion
}
