using NewLife.JT1078.FMp4;
using NewLife.JT1078.H264;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>fMP4 格式处理器。将 JTMessage 编码为 fMP4 Fragment 并推送给订阅者</summary>
public class Mp4Handler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    private FMp4Encoder _mp4Encoder;
    private H264Decoder _h264Decoder;
    private Byte[] _firstCache;
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public async Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType == DataTypes.AudioFrame) return 0;

        var _ = Hub;
        users = users?.Where(e => e.Type == RTPType.Mp4).ToArray();
        if (users == null || users.Length == 0) return 0;

        using var span = DefaultTracer.Instance?.NewSpan("Mp4Handler.Encode", msg);

        _mp4Encoder ??= new FMp4Encoder();
        _h264Decoder ??= new H264Decoder();

        var avframe = _h264Decoder.ParseAVFrame(msg);
        if (avframe.Nalus == null || avframe.Nalus.Count == 0) return 0;

        Byte[] first = null;
        Byte[] other = null;

        if (msg.DataType == DataTypes.VideoIFrame)
        {
            if (avframe.SPS != null && avframe.PPS != null)
            {
                _firstCache = _mp4Encoder.FirstVideoBox(avframe);
                first = _firstCache;
            }
        }

        if (_firstCache != null)
        {
            foreach (var session in users)
            {
                if (!session.HasStarted)
                {
                    if (msg.DataType == DataTypes.VideoIFrame && first != null)
                    {
                        await session.WriteAsync(first).ConfigureAwait(false);
                        session.HasStarted = true;
                    }
                }
                else
                {
                    other ??= _mp4Encoder.OtherVideoBox(avframe.Nalus, msg.GetKey(), msg.DataType == DataTypes.VideoIFrame);
                    if (other != null)
                        await session.WriteAsync(other).ConfigureAwait(false);
                }

                session.LastActive = DateTime.Now;
            }
        }

        return users.Length;
    }
    #endregion
}
