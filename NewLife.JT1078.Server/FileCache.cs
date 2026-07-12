using NewLife.Log;

namespace NewLife.JT1078.Server;

/// <summary>录制文件缓存。按日期目录存储音视频录制文件</summary>
public class FileCache : DisposeBase
{
    #region 属性
    /// <summary>缓存根目录</summary>
    public String RootPath { get; set; }

    /// <summary>当前文件流字典</summary>
    private readonly Dictionary<String, Stream> _files = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region 构造
    /// <summary>实例化文件缓存</summary>
    /// <param name="rootPath">缓存根目录</param>
    public FileCache(String rootPath = null)
    {
        RootPath = rootPath ?? Path.GetFullPath("./Cache");
        if (!Directory.Exists(RootPath))
            Directory.CreateDirectory(RootPath);
    }
    #endregion

    #region 方法
    /// <summary>获取存储路径（按日期分目录）</summary>
    private String GetFilePath(String type, String sim, Int32 channel)
    {
        var now = DateTime.Now;
        var dir = Path.Combine(RootPath, now.ToString("yyyyMMdd"));
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        var fileName = $"{now:HHmmss}_{sim}_{channel}_{type}";
        return Path.Combine(dir, fileName);
    }

    /// <summary>写入录制数据</summary>
    /// <param name="type">数据类型（flv/mp4/pcm/aac）</param>
    /// <param name="sim">SIM 卡号</param>
    /// <param name="channel">通道号</param>
    /// <param name="data">数据</param>
    /// <param name="offset">偏移</param>
    /// <param name="count">长度</param>
    public void Write(String type, String sim, Int32 channel, Byte[] data, Int32 offset = 0, Int32 count = -1)
    {
        var key = $"{type}_{sim}_{channel}";

        if (!_files.TryGetValue(key, out var fs))
        {
            var path = GetFilePath(type, sim, channel);
            fs = File.Create(path);
            _files[key] = fs;

            XTrace.Log.Info("FileCache.Create {0} -> {1}", key, path);
        }

        if (count < 0) count = data.Length - offset;
        fs.Write(data, offset, count);
    }

    /// <summary>完成录制，关闭文件流</summary>
    public void Finish(String type, String sim, Int32 channel)
    {
        var key = $"{type}_{sim}_{channel}";

        if (_files.TryGetValue(key, out var fs))
        {
            fs.Flush();
            fs.Dispose();
            _files.Remove(key);

            XTrace.Log.Info("FileCache.Finish {0}", key);
        }
    }

    /// <summary>获取已录制的文件列表</summary>
    public String[] GetFiles(String sim = null, Int32 channel = -1, DateTime? date = null)
    {
        var searchDate = date ?? DateTime.Now;
        var dir = Path.Combine(RootPath, searchDate.ToString("yyyyMMdd"));
        if (!Directory.Exists(dir)) return [];

        var files = Directory.GetFiles(dir);

        if (!sim.IsNullOrEmpty())
            files = files.Where(f => f.Contains(sim)).ToArray();

        if (channel >= 0)
            files = files.Where(f => f.Contains($"_{channel}_")).ToArray();

        return files;
    }

    /// <summary>释放资源</summary>
    protected override void Dispose(Boolean disposing)
    {
        base.Dispose(disposing);

        foreach (var fs in _files.Values)
        {
            fs.TryDispose();
        }
        _files.Clear();
    }
    #endregion
}
