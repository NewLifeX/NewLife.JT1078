using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>状态监控 ViewModel</summary>
public partial class StatusViewModel : ObservableObject
{
    private readonly ConnectionConfig _config;
    private HttpApiService? _api;
    private DispatcherTimer? _refreshTimer;

    /// <summary>实例化状态监控 ViewModel</summary>
    public StatusViewModel(ConnectionConfig config)
    {
        _config = config;
    }

    #region 属性
    /// <summary>设备列表</summary>
    public ObservableCollection<DeviceInfo> Devices { get; } = [];

    /// <summary>状态信息</summary>
    [ObservableProperty]
    private String _status = "就绪";

    /// <summary>是否自动刷新</summary>
    [ObservableProperty]
    private Boolean _isAutoRefresh;

    /// <summary>设备数量</summary>
    [ObservableProperty]
    private Int32 _deviceCount;
    #endregion

    /// <summary>刷新设备列表</summary>
    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            _api ??= new HttpApiService(_config);

            Status = "⏳ 查询中...";
            var devices = await _api.GetDevicesAsync();

            Devices.Clear();
            foreach (var d in devices)
                Devices.Add(d);

            DeviceCount = devices.Length;
            Status = $"已找到 {devices.Length} 个设备";
        }
        catch (Exception ex)
        {
            Status = $"❌ 查询失败: {ex.Message}";
        }
    }

    /// <summary>切换自动刷新</summary>
    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        IsAutoRefresh = !IsAutoRefresh;

        if (IsAutoRefresh)
        {
            // 每秒刷新一次
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _refreshTimer.Tick += async (_, _) => await Refresh();
            _refreshTimer.Start();
            Status = "自动刷新已开启（每 3 秒）";
        }
        else
        {
            _refreshTimer?.Stop();
            _refreshTimer = null;
            Status = "自动刷新已关闭";
        }
    }
}
