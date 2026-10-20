using LibVLCSharp.Shared;

namespace NewLife.JT1078.Client.Services;

/// <summary>基于 LibVLC 的流媒体播放器。负责原生库初始化、播放与释放</summary>
public sealed class VlcPlayerService : IDisposable
{
    private LibVLC? _libVlc;
    private MediaPlayer? _player;
    private Boolean _disposed;

    /// <summary>播放器实例，未初始化时为 null</summary>
    public MediaPlayer? Player => _player;

    /// <summary>最近一次初始化失败的原因</summary>
    public String? LastError { get; private set; }

    /// <summary>播放过程中发生错误时触发，参数为错误描述</summary>
    public event EventHandler<String>? ErrorOccurred;

    /// <summary>确保 LibVLC 与播放器已初始化。重复调用安全</summary>
    /// <returns>初始化成功返回 true，失败时原因写入 <see cref="LastError"/></returns>
    public Boolean EnsureInitialized()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(VlcPlayerService));
        if (_player != null) return true;

        if (!OperatingSystem.IsWindows())
        {
            LastError = "内置播放器仅自带 Windows 版 libvlc 原生库，其他平台请自行安装 LibVLC 运行时";
            return false;
        }

        try
        {
            Core.Initialize();
            _libVlc ??= new LibVLC("--no-video-title-show", "--network-caching=500");
            _player = new MediaPlayer(_libVlc);
            _player.EncounteredError += OnEncounteredError;
            LastError = null;

            return true;
        }
        catch (Exception ex)
        {
            LastError = $"LibVLC 初始化失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>播放指定地址</summary>
    /// <param name="url">流地址，支持 HTTP-FLV、HLS、HTTP 音频等</param>
    /// <returns>成功发起播放返回 true</returns>
    public Boolean Play(String url)
    {
        if (String.IsNullOrWhiteSpace(url)) return false;
        if (!EnsureInitialized()) return false;

        try
        {
            using var media = new Media(_libVlc!, new Uri(url));

            return _player!.Play(media);
        }
        catch (Exception ex)
        {
            LastError = $"播放失败：{ex.Message}";
            return false;
        }
    }

    /// <summary>停止播放</summary>
    public void Stop()
    {
        if (_player == null) return;

        try
        {
            _player.Stop();
        }
        catch (Exception ex)
        {
            LastError = $"停止失败：{ex.Message}";
        }
    }

    private void OnEncounteredError(Object? sender, EventArgs e) => ErrorOccurred?.Invoke(this, "LibVLC 播放失败，请确认设备已推流且流地址可访问");

    /// <summary>释放播放器与 LibVLC 资源</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_player != null)
        {
            _player.EncounteredError -= OnEncounteredError;
            _player.Stop();
            _player.Dispose();
            _player = null;
        }

        _libVlc?.Dispose();
        _libVlc = null;
    }
}
