using NewLife.JT1078.Flv;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>FLV 格式处理器。将 JTMessage 编码为 FLV Tag 并推送给订阅者</summary>
public class FlvHandler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    private FlvEncoder _flvEncoder;
    private Boolean _aacHeader = true;
    private Boolean _hasAudio;
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public async Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType == DataTypes.AudioFrame) return 0;

        var _ = Hub;
        users = users?.Where(e => e.Type == RTPType.Flv).ToArray();
        if (users == null || users.Length == 0) return 0;

        using var span = DefaultTracer.Instance?.NewSpan("FlvHandler.Encode", msg);

        _flvEncoder ??= new FlvEncoder(Hub.AudioCodec);

        // 首帧和其它帧分开编码
        Byte[] first = null;
        Byte[] other = null;

        foreach (var session in users)
        {
            if (!session.HasStarted)
            {
                if (msg.DataType == DataTypes.VideoIFrame)
                {
                    first ??= _flvEncoder.EncoderVideoTag(msg, true, _hasAudio);
                    if (first != null)
                    {
                        await session.WriteAsync(first).ConfigureAwait(false);
                        session.HasStarted = true;
                    }
                }
            }
            else
            {
                other ??= _flvEncoder.EncoderVideoTag(msg, false);
                if (other != null)
                    await session.WriteAsync(other).ConfigureAwait(false);
            }

            session.LastActive = DateTime.Now;
        }

        return users.Length;
    }
    #endregion
}
