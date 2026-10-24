using System.Collections.Concurrent;
using NewLife.JT1078.Protocols;
using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>消息中枢注册表。管理所有设备通道的消息中枢实例</summary>
/// <remarks>
/// 桥接设备侧（JT1078Session）和用户侧（HttpSession/LiveController）的数据流。
/// 每个设备通道（SIM_Channel）对应一个 <see cref="MsgHub"/> 实例。
/// </remarks>
public static class MsgHubRegistry
{
    /// <summary>中枢字典。Key = SIM_Channel</summary>
    /// <remarks>值用 Lazy 包装，保证并发首次访问只创建一个实例，避免工厂重复执行产生多余中枢</remarks>
    private static readonly ConcurrentDictionary<String, Lazy<MsgHub>> _hubs = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>获取或创建设备通道的消息中枢</summary>
    /// <param name="sim">SIM 卡号</param>
    /// <param name="channel">逻辑通道号</param>
    /// <returns>消息中枢实例</returns>
    public static MsgHub GetOrAdd(String sim, Int32 channel)
    {
        var key = $"{sim}_{channel}";
        var lazy = _hubs.GetOrAdd(key, k => new Lazy<MsgHub>(() =>
        {
            var hub = new MsgHub
            {
                SIM = sim,
                Channel = channel,
            };
            XTrace.Log.Info("MsgHubRegistry.Create {0}", k);
            return hub;
        }, LazyThreadSafetyMode.ExecutionAndPublication));

        return lazy.Value;
    }

    /// <summary>移除并关闭消息中枢</summary>
    /// <param name="sim">SIM 卡号</param>
    /// <param name="channel">逻辑通道号</param>
    public static void Remove(String sim, Int32 channel)
    {
        var key = $"{sim}_{channel}";
        if (_hubs.TryRemove(key, out var lazy))
        {
            // 创建后立即注销时值可能尚未创建，此时无需关闭
            if (lazy.IsValueCreated)
            {
                var hub = lazy.Value;
                hub.Close("HubRemoved");
                hub.Dispose();
            }
            XTrace.Log.Info("MsgHubRegistry.Remove {0}", key);
        }
    }

    /// <summary>获取所有消息中枢</summary>
    public static MsgHub[] GetAll() => _hubs.Values.Where(e => e.IsValueCreated).Select(e => e.Value).ToArray();

    /// <summary>获取指定设备通道的消息中枢</summary>
    public static MsgHub? Get(String sim, Int32 channel)
    {
        var key = $"{sim}_{channel}";
        return _hubs.TryGetValue(key, out var lazy) && lazy.IsValueCreated ? lazy.Value : null;
    }
}
