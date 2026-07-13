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
            Status = "⏳ 连接中...";
            IsPlaying = true;

            var format = Formats[SelectedFormat].ToLower();
            if (format == "fmp4") format = "mp4";

            // HLS 使用不同的端点
            String url;
            if (format == "hls")
            {
                url = $"{_config.WsBaseUrl}/hls.m3u8?sim={Sim}&channel={Channel}&token={_config.Token}";
                // HLS 通过 HTTP 获取，不需要 WebSocket
                Status = "ℹ️ HLS 请在外部播放器中打开此地址: " + url;
                IsPlaying = false;
                return;
            }

            if (SelectedMode == 1) // WebSocket
            {
                url = $"{_config.WsBaseUrl}/live.{format}?sim={Sim}&channel={Channel}&token={_config.Token}";
            }
            else // HTTP
            {
                url = $"{_config.HttpBaseUrl}/live.{format}?sim={Sim}&channel={Channel}&token={_config.Token}";
            }

            if (SelectedMode == 1)
            {
                _wsService = new WsStreamService();
                _wsService.OnDataReceived += OnDataReceived;
                _wsService.OnError += OnError;
                _wsService.OnConnectionChanged += OnConnectionChanged;
                await _wsService.ConnectAsync(url);
            }
            else
            {
                // HTTP 模式 - 打开链接到外部播放器或显示 URL
                Status = "ℹ️ HTTP 流地址: " + url;
                IsPlaying = false;
            }
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
        if (_wsService != null)
        {
            await _wsService.DisconnectAsync();
            _wsService.Dispose();
            _wsService = null;
        }

        IsPlaying = false;
        Status = "已停止";
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
