using NewLife;
using NewLife.JT1078.Models;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>HTTP/WS 拉流订阅者会话</summary>
public class HttpSession : DisposeBase, IMsgUser
{
    #region 属性
    private static Int32 _gid;
    private readonly Func<Byte[], ValueTask> _writeCallback;

    /// <summary>会话标识</summary>
    public Int32 Id { get; } = Interlocked.Increment(ref _gid);

    /// <summary>业务编码</summary>
    public String BizCode { get; set; }

    /// <summary>SIM 卡号</summary>
    public String SIM { get; set; }

    /// <summary>通道号</summary>
    public Int32 Channel { get; set; }

    /// <summary>用户标识</summary>
    public Int32 UserId { get; set; }

    /// <summary>用户名</summary>
    public String UserName { get; set; }

    /// <summary>是否已开始推送</summary>
    public Boolean HasStarted { get; set; }

    /// <summary>客户端 IP</summary>
    public String IP { get; set; }

    /// <summary>最后活跃时间</summary>
    public DateTime LastActive { get; set; } = DateTime.Now;

    /// <summary>播放模式</summary>
    public String Mode { get; set; }

    /// <summary>所属消息中枢</summary>
    public IMsgHub Hub { get; set; }

    /// <summary>是否仅音频</summary>
    public Boolean OnlyAudio { get; set; }

    /// <summary>是否需要音频</summary>
    public Boolean RequireAudio { get; set; }

    /// <summary>是否需要视频</summary>
    public Boolean RequireVideo { get; set; }

    /// <summary>开始时间</summary>
    public DateTime StartTime { get; set; } = DateTime.Now;

    /// <summary>流输出类型</summary>
    public RTPType Type { get; set; }
    #endregion

    #region 构造
    /// <summary>实例化 HTTP 订阅者会话</summary>
    /// <param name="writeCallback">写入回调，用于推送数据到 HTTP/WS 响应流</param>
    public HttpSession(Func<Byte[], ValueTask> writeCallback)
    {
        _writeCallback = writeCallback;
    }
    #endregion

    #region 方法
    /// <summary>关闭连接</summary>
    public void Close(String reason)
    {
        WriteLog("HttpSession.Close Id={0} Reason={1}", Id, reason);
        Hub?.RemoveUser(this);
        Hub = null;
    }

    /// <summary>分离消息中枢</summary>
    public void Detach(IMsgHub hub)
    {
        if (hub == Hub)
        {
            Hub = null;
            Hub?.RemoveUser(this);
        }
    }

    /// <summary>写入数据到输出流</summary>
    public async ValueTask WriteAsync(Byte[] data)
    {
        LastActive = DateTime.Now;

        if (_writeCallback != null)
            await _writeCallback(data).ConfigureAwait(false);
    }

    /// <summary>写日志</summary>
    public void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.Info("HttpSession." + format, args);
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Close("Dispose");
    }
    #endregion
}
