using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>设备命令 ViewModel</summary>
public partial class CommandViewModel : ObservableObject
{
    private readonly HttpApiService _api;

    /// <summary>实例化设备命令 ViewModel</summary>
    public CommandViewModel(ConnectionConfig config)
    {
        _api = new HttpApiService(config);
    }

    #region 属性
    /// <summary>SIM 卡号</summary>
    [ObservableProperty]
    private String _sim = "138001380000";

    /// <summary>逻辑通道号</summary>
    [ObservableProperty]
    private Int32 _channel = 1;

    /// <summary>AVType 索引</summary>
    [ObservableProperty]
    private Int32 _selectedAvType;

    /// <summary>DataType 索引</summary>
    [ObservableProperty]
    private Int32 _selectedDataType;

    /// <summary>十六进制负载数据</summary>
    [ObservableProperty]
    private String _hexData = "";

    /// <summary>发送结果</summary>
    [ObservableProperty]
    private String _result = "";

    /// <summary>设备列表</summary>
    public ObservableCollection<DeviceInfo> Devices { get; } = [];

    /// <summary>是否正在加载设备列表</summary>
    [ObservableProperty]
    private Boolean _isLoadingDevices;

    /// <summary>可用 AVTypes（下行常用）</summary>
    public String[] AVTypes { get; } =
    [
        "H264", "H265", "G711A", "G711U", "ADPCMA", "AAC", "MP3", "Transparent"
    ];
    #endregion

    /// <summary>刷新设备列表</summary>
    [RelayCommand]
    private async Task RefreshDevices()
    {
        IsLoadingDevices = true;
        try
        {
            var devices = await _api.GetDevicesAsync();
            Devices.Clear();
            foreach (var d in devices)
                Devices.Add(d);
            Result = $"找到 {devices.Length} 个设备";
        }
        catch (Exception ex)
        {
            Result = $"❌ 查询失败: {ex.Message}";
        }
        finally
        {
            IsLoadingDevices = false;
        }
    }

    /// <summary>发送命令</summary>
    [RelayCommand]
    private async Task SendCommand()
    {
        var avTypeStr = AVTypes[SelectedAvType];
        if (!Enum.TryParse<AVTypes>(avTypeStr, out var avType))
        {
            Result = "❌ 无效的 AVType";
            return;
        }

        var dataTypeStr = SelectedDataType switch
        {
            0 => "VideoIFrame",
            1 => "VideoPFrame",
            2 => "AudioFrame",
            3 => "Transparent",
            _ => "VideoIFrame"
        };
        if (!Enum.TryParse<DataTypes>(dataTypeStr, out var dataType))
        {
            Result = "❌ 无效的 DataType";
            return;
        }

        var (success, message) = await _api.SendCommandAsync(Sim, Channel, avType, dataType, HexData);
        Result = success ? $"✅ {message}" : $"❌ {message}";
    }
}
