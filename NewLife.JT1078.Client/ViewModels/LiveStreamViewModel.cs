using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>直播拉流 ViewModel</summary>
public partial class LiveStreamViewModel : ObservableObject
{
    private readonly ConnectionConfig _config;
    private WsStreamService? _wsService;

    /// <summary>实例化直播拉流 ViewModel</summary>
    public LiveStreamViewModel(ConnectionConfig config)
    {
        _config = config;
    }

    #region 属性
    /// <summary>SIM 卡号</summary>
    [ObservableProperty]
    private String _sim = "138001380000";

    /// <summary>逻辑通道号</summary>
    [ObservableProperty]
    private Int32 _channel = 1;

    /// <summary>流格式</summary>
    [ObservableProperty]
    private Int32 _selectedFormat;

    /// <summary>连接模式。0=HTTP, 1=WebSocket</summary>
    [ObservableProperty]
    private Int32 _selectedMode;

    /// <summary>是否正在播放</summary>
    [ObservableProperty]
    private Boolean _isPlaying;

    /// <summary>状态信息</summary>
    [ObservableProperty]
    private String _status = "就绪";

    /// <summary>已接收字节数</summary>
    [ObservableProperty]
    private Int64 _receivedBytes;

    /// <summary>已接收包数</summary>
    [ObservableProperty]
    private Int64 _receivedPackets;

    /// <summary>码率（bps）</summary>
    [ObservableProperty]
    private Double _bitrate;

    /// <summary>持续时间（秒）</summary>
    [ObservableProperty]
    private Int32 _duration;

    /// <summary>内置播放器拉流地址，为空表示未播放</summary>
    [ObservableProperty]
    private String? _playbackUrl;

    /// <summary>是否由内置播放器渲染</summary>
    [ObservableProperty]
    private Boolean _isPlayerActive;

    /// <summary>播放区域提示文本</summary>
    [ObservableProperty]
    private String _playerHint = "视频播放区域：选择 FLV / fMP4 / HLS 等格式，使用 HTTP 模式后点击播放";

    /// <summary>日志列表</summary>
    public ObservableCollection<MessageLogEntry> LogEntries { get; } = [];

    /// <summary>可用格式列表</summary>
    public String[] Formats { get; } = ["FLV", "fMP4", "PCM", "AAC", "WAV", "MP3", "HLS"];

    /// <summary>连接模式列表</summary>
    public String[] Modes { get; } = ["HTTP", "WebSocket"];
    #endregion

    partial void OnSelectedFormatChanged(Int32 value)
    {
        // HLS 只能 HTTP，不能 WebSocket
        if (value == 6 && SelectedMode == 1)
            SelectedMode = 0;
    }

    /// <summary>播放</summary>
    [RelayCommand]
    private async Task Play()
    {
        if (IsPlaying) return;

        try
        {
            var format = Formats[SelectedFormat].ToLowerInvariant();

            // HLS 走 HTTP 播放列表，WebSocket 模式无法承载
            if (format == "hls" && SelectedMode == 1)
            {
                Status = "ℹ️ HLS 仅支持 HTTP 模式，请切换到 HTTP 后播放";
                return;
            }

            Status = "⏳ 连接中...";
            IsPlaying = true;
            StreamStartTime = DateTime.Now;

            if (SelectedMode == 0) // HTTP
            {
                var url = LiveUrlBuilder.BuildHttpUrl(_config, format, Sim, Channel);
                if (LiveUrlBuilder.IsPlayable(format))
                {
                    PlaybackUrl = url;
                    IsPlayerActive = true;
                    Status = "▶ 播放中（内置播放器）：" + url;
                }
                else
                {
                    // PCM 等裸流无法由播放器解析，仅提示外部播放器地址
                    Status = "ℹ️ 该格式为裸流，请用外部播放器打开：" + url;
                    IsPlaying = false;
                }

                return;
            }

            // WebSocket 模式
            var fmt = format == "fmp4" ? "mp4" : format;
            var wsUrl = $"{_config.WsBaseUrl}/live.{fmt}?sim={Sim}&channel={Channel}&token={_config.Token}";

            _wsService = new WsStreamService();
            _wsService.OnDataReceived += OnDataReceived;
            _wsService.OnError += OnError;
            _wsService.OnConnectionChanged += OnConnectionChanged;
            await _wsService.ConnectAsync(wsUrl);
        }
        catch (Exception ex) when (ex is ArgumentException or ArgumentOutOfRangeException)
        {
            Status = $"❌ 参数错误: {ex.Message}";
            IsPlaying = false;
        }
        catch (Exception ex)
        {
            Status = $"❌ 连接失败: {ex.Message}";
            IsPlaying = false;
        }
    }

    /// <summary>停止</summary>
    [RelayCommand]
    private async Task Stop()
    {
        await StopInternalAsync();
        Status = "已停止";
    }

    /// <summary>停止播放并复位播放状态，供视图在离开页面时调用</summary>
    public void StopPlayback()
    {
        PlaybackUrl = null;
        IsPlayerActive = false;
        IsPlaying = false;
    }

    /// <summary>回报内置播放器错误并复位播放状态</summary>
    /// <param name="message">错误描述</param>
    public void ReportPlayerError(String message)
    {
        StopPlayback();
        Status = "❌ " + message;
    }

    private async Task StopInternalAsync()
    {
        if (_wsService != null)
        {
            await _wsService.DisconnectAsync();
            _wsService.Dispose();
            _wsService = null;
        }

        StopPlayback();
    }

    private void OnDataReceived(Byte[] data)
    {
        ReceivedBytes += data.Length;
        ReceivedPackets++;
        Duration = (Int32)(DateTime.Now - StreamStartTime).TotalSeconds;

        // 估算码率（每秒更新）
        if (Duration > 0)
            Bitrate = ReceivedBytes * 8.0 / Duration;
    }

    private DateTime StreamStartTime = DateTime.Now;

    private void OnError(Exception ex)
    {
        Status = $"❌ 错误: {ex.Message}";
        IsPlaying = false;
    }

    private void OnConnectionChanged(Boolean connected)
    {
        if (connected)
        {
            StreamStartTime = DateTime.Now;
            Status = "▶ 播放中...";
        }
        else
        {
            Status = "已断开";
            IsPlaying = false;
        }
    }
}
