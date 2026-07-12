using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>设备在线状态</summary>
public class DeviceStatus
{
    /// <summary>SIM 卡号</summary>
    public String SIM { get; set; }

    /// <summary>通道号</summary>
    public Int32 Channel { get; set; }

    /// <summary>是否在线</summary>
    public Boolean Online { get; set; }

    /// <summary>上线时间</summary>
    public DateTime OnlineTime { get; set; }

    /// <summary>最后活跃时间</summary>
    public DateTime LastActive { get; set; }

    /// <summary>客户端地址</summary>
    public String RemoteAddress { get; set; }

    /// <summary>当前订阅者数</summary>
    public Int32 SubscriberCount { get; set; }

    /// <summary>音频编码类型</summary>
    public String AudioType { get; set; }

    /// <summary>视频编码类型</summary>
    public String VideoType { get; set; }

    /// <summary>下行流量（字节）</summary>
    public Int64 DownlinkBytes { get; set; }
}

/// <summary>设备在线状态服务</summary>
public class StatusService : DisposeBase
{
    #region 属性
    private readonly Dictionary<String, DeviceStatus> _devices = new(StringComparer.OrdinalIgnoreCase);
    private readonly ReaderWriterLockSlim _lock = new();
    #endregion

    #region 方法
    /// <summary>设备上线</summary>
    public void Online(String sim, Int32 channel, String remoteAddress)
    {
        var key = $"{sim}_{channel}";
        var status = new DeviceStatus
        {
            SIM = sim,
            Channel = channel,
            Online = true,
            OnlineTime = DateTime.Now,
            LastActive = DateTime.Now,
            RemoteAddress = remoteAddress,
        };

        _lock.EnterWriteLock();
        try
        {
            _devices[key] = status;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        XTrace.Log.Info("StatusService.Online {0} Ch={1} {2}", sim, channel, remoteAddress);
    }

    /// <summary>设备离线</summary>
    public void Offline(String sim, Int32 channel)
    {
        var key = $"{sim}_{channel}";

        _lock.EnterWriteLock();
        try
        {
            if (_devices.TryGetValue(key, out var status))
                status.Online = false;
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        XTrace.Log.Info("StatusService.Offline {0} Ch={1}", sim, channel);
    }

    /// <summary>心跳</summary>
    public void Touch(String sim, Int32 channel)
    {
        var key = $"{sim}_{channel}";

        _lock.EnterReadLock();
        try
        {
            if (_devices.TryGetValue(key, out var status))
                status.LastActive = DateTime.Now;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>获取所有在线设备</summary>
    public DeviceStatus[] GetAll()
    {
        _lock.EnterReadLock();
        try
        {
            return _devices.Values.Where(e => e.Online).ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>获取指定设备状态</summary>
    public DeviceStatus Get(String sim, Int32 channel)
    {
        var key = $"{sim}_{channel}";

        _lock.EnterReadLock();
        try
        {
            _devices.TryGetValue(key, out var status);
            return status;
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>更新订阅者数</summary>
    public void SetSubscriberCount(String sim, Int32 channel, Int32 count)
    {
        var key = $"{sim}_{channel}";

        _lock.EnterWriteLock();
        try
        {
            if (_devices.TryGetValue(key, out var status))
                status.SubscriberCount = count;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>更新下行流量</summary>
    public void AddBytes(String sim, Int32 channel, Int64 bytes)
    {
        var key = $"{sim}_{channel}";

        _lock.EnterWriteLock();
        try
        {
            if (_devices.TryGetValue(key, out var status))
                status.DownlinkBytes += bytes;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>获取通道状态指标</summary>
    public String GetMetrics(String sim, Int32 channel)
    {
        var status = Get(sim, channel);
        if (status == null) return null;

        return $"SIM={status.SIM} Ch={status.Channel} " +
               $"Online={status.Online} " +
               $"Active={status.LastActive:HH:mm:ss} " +
               $"Subs={status.SubscriberCount} " +
               $"Bytes={status.DownlinkBytes} " +
               $"Addr={status.RemoteAddress}";
    }

    /// <summary>清理超时设备</summary>
    public void Cleanup(Int32 timeoutSeconds = 60)
    {
        var timeout = DateTime.Now.AddSeconds(-timeoutSeconds);
        var expired = new List<String>();

        _lock.EnterReadLock();
        try
        {
            foreach (var kv in _devices)
            {
                if (kv.Value.Online && kv.Value.LastActive < timeout)
                    expired.Add(kv.Key);
            }
        }
        finally
        {
            _lock.ExitReadLock();
        }

        _lock.EnterWriteLock();
        try
        {
            foreach (var key in expired)
            {
                if (_devices.TryGetValue(key, out var status))
                    status.Online = false;
            }
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);
        _lock.Dispose();
    }
    #endregion
}
