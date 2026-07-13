using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>录像回放 ViewModel</summary>
public partial class RecordViewModel : ObservableObject
{
    private readonly ConnectionConfig _config;

    /// <summary>实例化录像回放 ViewModel</summary>
    public RecordViewModel(ConnectionConfig config)
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

    /// <summary>开始时间</summary>
    [ObservableProperty]
    private String _startTime = DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>结束时间</summary>
    [ObservableProperty]
    private String _endTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>是否正在播放</summary>
    [ObservableProperty]
    private Boolean _isPlaying;

    /// <summary>状态信息</summary>
    [ObservableProperty]
    private String _status = "就绪";

    /// <summary>可用格式列表</summary>
    public String[] Formats { get; } = ["FLV", "fMP4", "AAC", "PCM", "WAV"];
    #endregion

    /// <summary>播放录像</summary>
    [RelayCommand]
    private void Play()
    {
        var format = Formats[SelectedFormat].ToLower();
        if (format == "fmp4") format = "mp4";

        var url = $"{_config.HttpBaseUrl}/record.{format}?sim={Sim}&channel={Channel}";
        Status = $"ℹ️ 录像流地址: {url}";
        IsPlaying = true;
    }

    /// <summary>停止</summary>
    [RelayCommand]
    private void Stop()
    {
        IsPlaying = false;
        Status = "已停止";
    }
}
