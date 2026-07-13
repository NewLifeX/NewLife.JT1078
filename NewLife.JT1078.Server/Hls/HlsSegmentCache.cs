using System.Collections.Concurrent;

namespace NewLife.JT1078.Server.Hls;

/// <summary>HLS 段缓存。管理每个设备通道的 TS 段</summary>
/// <remarks>
/// 线程安全，支持并发读写。
/// 每个设备通道维护一个滑动窗口（最多保留 20 个段）。
/// </remarks>
public class HlsSegmentCache : IDisposable
{
    #region 属性
    /// <summary>默认全局实例</summary>
    public static HlsSegmentCache Default { get; } = new();

    /// <summary>最大保留段数</summary>
    public Int32 MaxSegments { get; set; } = 20;

    /// <summary>段时长（秒）</summary>
    public Double TargetDuration { get; set; } = 2.0;

    /// <summary>每个设备通道的段列表</summary>
    private readonly ConcurrentDictionary<String, SegmentRingBuffer> _buffers = new(StringComparer.OrdinalIgnoreCase);

    private Boolean _disposed;
    #endregion

    #region 方法
    /// <summary>获取设备通道的 Key</summary>
    public static String GetKey(String sim, Int32 channel) => $"{sim}_{channel}";

    /// <summary>添加 TS 段</summary>
    /// <param name="key">设备通道键（SIM_Channel）</param>
    /// <param name="data">TS 段二进制数据</param>
    /// <param name="duration">段时长（秒）</param>
    /// <returns>段序号</returns>
    public Int32 AddSegment(String key, Byte[] data, Double duration)
    {
        var buffer = _buffers.GetOrAdd(key, _ => new SegmentRingBuffer(MaxSegments));
        return buffer.Add(data, duration);
    }

    /// <summary>获取指定序号的段</summary>
    public HlsSegmentInfo GetSegment(String key, Int32 sequence)
    {
        if (_buffers.TryGetValue(key, out var buffer))
        {
            return buffer.Get(sequence);
        }
        return null;
    }

    /// <summary>获取播放列表</summary>
    public String GetPlaylist(String key)
    {
        if (_buffers.TryGetValue(key, out var buffer))
        {
            var segments = buffer.GetAll();
            if (segments.Count == 0) return null;

            return M3U8Writer.WritePlaylist(segments, TargetDuration);
        }
        return null;
    }

    /// <summary>获取当前最新段序号</summary>
    public Int32 GetCurrentSequence(String key)
    {
        if (_buffers.TryGetValue(key, out var buffer))
        {
            return buffer.CurrentSequence;
        }
        return -1;
    }

    /// <summary>清理指定设备通道的缓存</summary>
    public void Remove(String key)
    {
        if (_buffers.TryRemove(key, out var buffer))
        {
            buffer.Clear();
        }
    }

    /// <summary>清理所有缓存</summary>
    public void Clear()
    {
        foreach (var buffer in _buffers.Values)
        {
            buffer.Clear();
        }
        _buffers.Clear();
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Clear();
    }
    #endregion

    #region 内部类
    /// <summary>段环形缓冲区（线程安全）</summary>
    private class SegmentRingBuffer
    {
        private readonly Int32 _capacity;
        private readonly HlsSegmentInfo[] _items;
        private readonly Object _lock = new();
        private Int32 _nextIndex;
        private Int32 _nextSequence;

        public Int32 CurrentSequence => _nextSequence - 1;

        public SegmentRingBuffer(Int32 capacity)
        {
            _capacity = capacity;
            _items = new HlsSegmentInfo[capacity];
            _nextIndex = 0;
            _nextSequence = 0;
        }

        public Int32 Add(Byte[] data, Double duration)
        {
            lock (_lock)
            {
                var seq = _nextSequence++;
                var segment = new HlsSegmentInfo
                {
                    Sequence = seq,
                    Duration = duration,
                    Url = $"{seq}.ts",
                    Data = data,
                };

                _items[_nextIndex] = segment;
                _nextIndex = (_nextIndex + 1) % _capacity;

                return seq;
            }
        }

        public HlsSegmentInfo Get(Int32 sequence)
        {
            lock (_lock)
            {
                // Find the segment by sequence number
                var startSeq = _nextSequence - _capacity;
                if (sequence < startSeq || sequence >= _nextSequence)
                    return null;

                var idx = (sequence % _capacity + _capacity) % _capacity;
                var item = _items[idx];
                if (item != null && item.Sequence == sequence)
                    return item;

                return null;
            }
        }

        public List<HlsSegmentInfo> GetAll()
        {
            lock (_lock)
            {
                var list = new List<HlsSegmentInfo>(_capacity);
                var count = Math.Min(_nextSequence, _capacity);
                for (var i = 0; i < count; i++)
                {
                    var idx = (_nextIndex - count + i + _capacity) % _capacity;
                    if (_items[idx] != null)
                        list.Add(_items[idx]);
                }
                return list;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                for (var i = 0; i < _items.Length; i++) _items[i] = null;
                _nextIndex = 0;
                _nextSequence = 0;
            }
        }
    }
    #endregion
}
