using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>订阅者管理器。管理 HTTP/WS 拉流客户端的生命周期</summary>
public class SessionManager : DisposeBase
{
    #region 属性
    /// <summary>订阅者列表</summary>
    private readonly List<IMsgUser> _sessions = new();

    /// <summary>读写锁</summary>
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>空闲超时秒数。超过此时间无活跃则自动移除</summary>
    public Int32 IdleTimeout { get; set; } = 30;

    /// <summary>清理周期（秒）</summary>
    public Int32 CleanInterval { get; set; } = 10;
    #endregion

    #region 方法
    /// <summary>添加订阅者</summary>
    public void Add(IMsgUser session)
    {
        _lock.EnterWriteLock();
        try
        {
            _sessions.Add(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>移除订阅者</summary>
    public Boolean Remove(IMsgUser session)
    {
        _lock.EnterWriteLock();
        try
        {
            return _sessions.Remove(session);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    /// <summary>获取指定设备通道的所有订阅者</summary>
    public IMsgUser[] GetAll(String sim, Int32 channel)
    {
        _lock.EnterReadLock();
        try
        {
            return _sessions.Where(e =>
                e.SIM.EqualIgnoreCase(sim) &&
                e.Channel == channel &&
                !e.Disposed).ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    /// <summary>清理超时订阅者</summary>
    public void Cleanup()
    {
        var timeout = DateTime.Now.AddSeconds(-IdleTimeout);
        var expired = new List<IMsgUser>();

        _lock.EnterReadLock();
        try
        {
            expired.AddRange(_sessions.Where(e =>
                e.LastActive < timeout &&
                !e.Disposed));
        }
        finally
        {
            _lock.ExitReadLock();
        }

        foreach (var session in expired)
        {
            XTrace.Log.Info("SessionManager.Cleanup Id={0} SIM={1} Ch={2}", session.Id, session.SIM, session.Channel);
            session.Close("IdleTimeout");
            Remove(session);
        }
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        _lock.EnterWriteLock();
        try
        {
            foreach (var session in _sessions)
            {
                session.Close("Dispose");
            }
            _sessions.Clear();
        }
        finally
        {
            _lock.ExitWriteLock();
        }

        _lock.Dispose();
    }
    #endregion
}
