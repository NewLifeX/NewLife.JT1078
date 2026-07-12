using System.Collections.Concurrent;
using NewLife.Caching;
using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Log;
using NewLife.Messaging;

namespace NewLife.JT1078.Server;

/// <summary>JT1078 设备命令总线。通过事件总线实现集群范围的设备命令下发与信令交互</summary>
/// <remarks>
/// 核心流程（集群多实例场景）：
/// <list type="bullet">
/// <item>任一实例调用 <see cref="PublishAsync"/> 发起命令，通过事件总线「广播」到全部实例</item>
/// <item>各实例的 <see cref="OnMessage"/> 收到广播后，查找本地是否有目标设备会话</item>
/// <item>持有设备会话的实例通过 <see cref="JT1078Session.SendJT"/> 下发给设备</item>
/// </list>
/// 
/// 事件总线模式：
/// - 单机模式：使用内存 <see cref="EventBus{T}"/>，仅支持进程内分发
/// - 集群模式：通过 <see cref="ICacheProvider"/> 创建 Redis 事件总线，支持跨进程消息分发
/// 
/// 消息格式：SIM_Channel#AVType|DataType|HexPayload
/// </remarks>
public class DeviceSessionManager : DisposeBase
{
    #region 属性
    /// <summary>主题。事件总线的频道名称，默认 JT1078:Commands</summary>
    public String Topic { get; set; } = "JT1078:Commands";

    /// <summary>客户端标识。机器名+进程号</summary>
    public String ClientId { get; set; } = Runtime.ClientId;

    /// <summary>事件总线。用于跨进程消息分发，单机模式使用内存总线，集群模式使用 Redis 总线</summary>
    public IEventBus<String> Bus { get; set; }

    /// <summary>缓存提供者。用于创建 Redis 事件总线（由 Stardust AppClient 或 Redis 提供）</summary>
    public ICacheProvider CacheProvider { get; set; }

    /// <summary>本地设备会话字典。Key = SIM_Channel</summary>
    private readonly ConcurrentDictionary<String, Object> _sessions = new(StringComparer.OrdinalIgnoreCase);

    private readonly ITracer _tracer;
    private readonly IServiceProvider _provider;
    private Boolean _inited;

#if NET45
    private static readonly Task _completedTask = TaskEx.FromResult(0);
#else
    private static readonly Task _completedTask = Task.CompletedTask;
#endif
    #endregion

    #region 构造
    /// <summary>实例化设备命令总线</summary>
    public DeviceSessionManager(IServiceProvider serviceProvider)
    {
        _provider = serviceProvider;
        _tracer = serviceProvider.GetService(typeof(ITracer)) as ITracer;
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _sessions.Clear();
        (Bus as IDisposable)?.Dispose();
    }
    #endregion

    #region 初始化
    /// <summary>初始化事件总线。首次使用时会自动调用</summary>
    public void Init()
    {
        if (_inited) return;
        lock (this)
        {
            if (_inited) return;
            _inited = true;

            // 优先使用外部注入的缓存提供者（Stardust/Redis 集群总线）
            if (CacheProvider != null)
                Bus = CacheProvider.CreateEventBus<String>(Topic, ClientId);
            // 次选从 DI 容器解析缓存提供者
            if (Bus == null)
            {
                var cacheProvider = _provider.GetService(typeof(ICacheProvider)) as ICacheProvider;
                if (cacheProvider != null)
                    Bus = cacheProvider.CreateEventBus<String>(Topic, ClientId);
            }
            // 回退到内存总线（仅进程内）
            Bus ??= new EventBus<String>();

            EventBusExtensions.Subscribe(Bus, OnMessage);

            WriteLog("Init Topic={0} Bus={1}", Topic, Bus.GetType().Name);
        }
    }
    #endregion

    #region 会话管理
    /// <summary>注册设备会话</summary>
    /// <param name="key">设备标识，格式 SIM_Channel</param>
    /// <param name="session">设备会话（JT1078Session 或 JT1078UdpSession）</param>
    public void Register(String key, Object session)
    {
        _sessions[key] = session;
        WriteLog("Register {0}", key);
    }

    /// <summary>注销设备会话</summary>
    /// <param name="key">设备标识</param>
    public void Unregister(String key)
    {
        if (_sessions.TryRemove(key, out _))
        {
            WriteLog("Unregister {0}", key);
        }
    }

    /// <summary>获取本地设备会话</summary>
    public Object GetSession(String key)
    {
        _sessions.TryGetValue(key, out var session);
        return session;
    }

    /// <summary>获取所有已注册的本地设备</summary>
    public Object[] GetSessions() => _sessions.Values.ToArray();

    /// <summary>是否存在指定设备的本地会话</summary>
    public Boolean Contains(String key) => _sessions.ContainsKey(key);
    #endregion

    #region 命令下发
    /// <summary>向设备下发信令。优先本地查找，未命中则广播到集群</summary>
    /// <param name="sim">SIM 卡号</param>
    /// <param name="channel">通道号</param>
    /// <param name="avtype">音视频编码类型</param>
    /// <param name="dataType">数据类型（下行信令）</param>
    /// <param name="payload">负载数据</param>
    /// <returns>true 表示已处理（本地发送或已广播）</returns>
    public Boolean Publish(String sim, Int32 channel, AVTypes avtype, DataTypes dataType, IPacket payload)
    {
        if (!_inited) Init();

        var key = $"{sim}_{channel}";

        // 先尝试本地发送
        if (_sessions.TryGetValue(key, out var sessionObj) && sessionObj is JT1078Session session)
        {
            session.SendJT(avtype, dataType, payload);
            WriteLog("Publish 本地发送 key={0} type={1}", key, dataType);
            return true;
        }

        // 没有本地会话，广播到集群
        if (Bus != null)
        {
            var hex = payload?.GetSpan().ToArray();
            var command = $"{avtype}|{(Int32)dataType}|{hex}";
            var message = $"{key}#{command}";

            Bus.PublishAsync(message, null, default);
            WriteLog("Publish 广播 key={0} message={1}", key, message);
        }
        return true;
    }

    /// <summary>异步向设备下发信令。优先本地查找，未命中则广播到集群</summary>
    public async Task<Boolean> PublishAsync(String sim, Int32 channel, AVTypes avtype, DataTypes dataType, IPacket payload, CancellationToken cancellationToken = default)
    {
        if (!_inited) Init();

        var key = $"{sim}_{channel}";

        // 先尝试本地发送
        if (_sessions.TryGetValue(key, out var sessionObj) && sessionObj is JT1078Session session)
        {
            session.SendJT(avtype, dataType, payload);
            WriteLog("PublishAsync 本地发送 key={0} type={1}", key, dataType);
            return true;
        }

        // 没有本地会话，广播到集群
        if (Bus != null)
        {
            var hex = payload?.GetSpan().ToArray();
            var command = $"{avtype}|{(Int32)dataType}|{hex}";
            var message = $"{key}#{command}";

            await Bus.PublishAsync(message, null, cancellationToken).ConfigureAwait(false);
            WriteLog("PublishAsync 广播 key={0}", key);
        }
        return true;
    }
    #endregion

    #region 事件处理
    /// <summary>收到集群广播的命令</summary>
    private Task OnMessage(String message, IEventContext context, CancellationToken cancellationToken)
    {
        using var span = _tracer?.NewSpan("DeviceSessionManager.OnMessage", message);

        var p = message.IndexOf('#');
        if (p <= 0) return _completedTask;

        var key = message[..p];
        var command = message[(p + 1)..];

        // 查找本地是否有目标会话
        if (!(_sessions.TryGetValue(key, out var sessionObj) && sessionObj is JT1078Session session))
            return _completedTask;

        // 解析命令并转发给设备
        var parts = command.Split('|');
        if (parts.Length >= 3 &&
            Enum.TryParse<AVTypes>(parts[0], out var avtype) &&
            Int32.TryParse(parts[1], out var dt))
        {
            var dataType = (DataTypes)dt;
            var hex = parts[2];
            IPacket data = null;
            if (!String.IsNullOrEmpty(hex) && hex.Length > 2)
                data = new ArrayPacket(hex.ToHex());

            session.SendJT(avtype, dataType, data);
            WriteLog("OnMessage 命中本地会话 key={0} type={1}", key, dataType);
        }

        return _completedTask;
    }
    #endregion

    #region 辅助
    private void WriteLog(String format, params Object[] args)
    {
        XTrace.Log.Info("DeviceSessionManager." + String.Format(format, args));
    }
    #endregion
}
