using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>HLS 直播格式处理器。将音视频帧切片为 TS 分片并生成 M3U8 播放列表</summary>
/// <remarks>
/// 当前为桩代码状态，需要 HLS 封装库支持（如 m3u8 播放列表生成 + TS 切片器）。
/// 未引入 HLS 封装库时，EncodeAsync 返回 0 表示不处理。
/// </remarks>
public class HlsHandler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.VideoIFrame && msg.DataType != DataTypes.VideoPFrame && msg.DataType != DataTypes.VideoBFrame && msg.DataType != DataTypes.AudioFrame)
            return Task.FromResult(0);

        users = users?.Where(e => e.Type == RTPType.Hls).ToArray();
        if (users == null || users.Length == 0) return Task.FromResult(0);

        WriteLog("HlsHandler.EncodeAsync HLS 封装器未实现，跳过");

        return Task.FromResult(0);
    }
    #endregion

    #region 辅助
    private void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.Warn("HlsHandler." + String.Format(format, args));
    }
    #endregion
}
