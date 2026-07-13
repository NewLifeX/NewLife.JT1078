using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>MP3 音频格式处理器。将音频帧解码为 PCM 后编码为 MP3</summary>
/// <remarks>
/// 当前为桩代码状态，需要外部 MP3 编码器库（如 LAME）支持。
/// 未引入 MP3 编码器时，EncodeAsync 返回 0 表示不处理。
/// </remarks>
public class Mp3Handler : IMsgHandler
{
    #region 属性
    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }
    #endregion

    #region 方法
    /// <summary>编码消息并推送</summary>
    public Task<Int32> EncodeAsync(JTMessage msg, IMsgUser[] users)
    {
        if (msg.DataType != DataTypes.AudioFrame) return Task.FromResult(0);

        users = users?.Where(e => e.Type == RTPType.Mp3).ToArray();
        if (users == null || users.Length == 0) return Task.FromResult(0);

        WriteLog("Mp3Handler.EncodeAsync MP3 编码器未实现，跳过");

        return Task.FromResult(0);
    }
    #endregion

    #region 辅助
    private void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.Warn("Mp3Handler." + String.Format(format, args));
    }
    #endregion
}
