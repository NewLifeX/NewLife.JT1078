using NewLife.Audio;
using NewLife.Collections;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.JT1078.Server;

/// <summary>消息中枢。管理设备侧连接与用户拉流侧会话之间的数据分发</summary>
public class MsgHub : DisposeBase, IMsgHub
{
    #region 属性
    /// <summary>终端 SIM 卡号</summary>
    public String SIM { get; set; }

    /// <summary>逻辑通道号</summary>
    public Int32 Channel { get; set; }

    /// <summary>设备会话</summary>
    public Object Device { get; set; }

    /// <summary>空闲超时秒数</summary>
    public Int32 IdleTime { get; set; } = 30;

    /// <summary>音频编解码器工厂</summary>
    public AudioCodecFactory AudioCodec { get; } = new();

    /// <summary>已连接的订阅者集合</summary>
    private readonly ConcurrentHashSet<IMsgUser> _users = new();

    /// <summary>文件缓存流字典</summary>
    private readonly Dictionary<String, MemoryStream> _files = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>格式处理器缓存。按 RTPType 延迟创建</summary>
    private readonly Dictionary<RTPType, IMsgHandler> _handlers = new();

    /// <summary>是否已启动</summary>
    private Boolean _started;
    #endregion

    #region 方法
    /// <summary>开始处理指定通道的数据</summary>
    public void Start(String sim, Int32 channel, DataTypes type)
    {
        if (_started) return;
        _started = true;

        SIM = sim;
        Channel = channel;

        WriteLog("MsgHub.Start {0} Ch={1} Type={2}", sim, channel, type);
    }

    /// <summary>关闭通道，断开所有订阅者</summary>
    public void Close(String reason)
    {
        WriteLog("MsgHub.Close {0} Ch={1} Reason={2}", SIM, Channel, reason);

        foreach (var user in _users.ToArray())
        {
            user.Detach(this);
            user.Close(reason);
        }

        foreach (var file in _files.Values)
        {
            file.TryDispose();
        }
        _files.Clear();

        foreach (var handler in _handlers.Values)
        {
            (handler as IDisposable)?.Dispose();
        }
        _handlers.Clear();
    }

    /// <summary>生产消息，分发给匹配的订阅者</summary>
    public async Task<Int32> ProduceAsync(JTMessage msg, CancellationToken cancellationToken = default)
    {
        var users = _users.ToArray();
        if (users.Length == 0) return 0;

        // 按 RTPType 分组，每组调用对应 handler
        var groups = users.GroupBy(e => e.Type);
        var total = 0;
        foreach (var group in groups)
        {
            var handler = GetOrCreateHandler(group.Key);
            if (handler != null)
                total += await handler.EncodeAsync(msg, group.ToArray()).ConfigureAwait(false);
        }

        return total;
    }

    /// <summary>获取或创建格式处理器</summary>
    private IMsgHandler? GetOrCreateHandler(RTPType type)
    {
        if (_handlers.TryGetValue(type, out var handler)) return handler;

        handler = type switch
        {
            RTPType.Flv => new FlvHandler { Hub = this },
            RTPType.Mp4 => new Mp4Handler { Hub = this },
            RTPType.Aac => new AacHandler { Hub = this },
            RTPType.Pcm => new PcmHandler { Hub = this },
            RTPType.Wav => new WavHandler { Hub = this },
            RTPType.Hls => new HlsHandler { Hub = this },
            RTPType.Mp3 => new Mp3Handler { Hub = this },
            _ => null,
        };

        if (handler != null)
            _handlers[type] = handler;

        return handler;
    }

    /// <summary>向所有订阅者写入数据</summary>
    public async ValueTask WriteAsync(Byte[] data)
    {
        foreach (var user in _users.ToArray())
        {
            try
            {
                await user.WriteAsync(data).ConfigureAwait(false);
            }
            catch
            {
                user.Close("WriteError");
                _users.TryRemove(user);
            }
        }
    }

    /// <summary>写入文件缓存</summary>
    public void WriteFile(String type, Byte[] data, Int32 offset = 0, Int32 count = -1)
    {
        if (!_files.TryGetValue(type, out var fs))
        {
            fs = new MemoryStream();
            _files[type] = fs;
        }

        if (count < 0) count = data.Length - offset;
        fs.Write(data, offset, count);
    }

    /// <summary>获取文件缓存流</summary>
    public Stream GetFile(String type)
    {
        if (!_files.TryGetValue(type, out var fs))
        {
            fs = new MemoryStream();
            _files[type] = fs;
        }
        return fs;
    }

    /// <summary>添加订阅者</summary>
    public Boolean AddUser(IMsgUser user)
    {
        user.Hub = this;
        return _users.TryAdd(user);
    }

    /// <summary>移除订阅者</summary>
    public Boolean RemoveUser(IMsgUser user) => _users.TryRemove(user);

    /// <summary>获取所有订阅者</summary>
    public IMsgUser[] GetUsers() => _users.ToArray();

    /// <summary>写日志</summary>
    public void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.Info("MsgHub." + format, args);
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        Close("Dispose");
    }
    #endregion
}
