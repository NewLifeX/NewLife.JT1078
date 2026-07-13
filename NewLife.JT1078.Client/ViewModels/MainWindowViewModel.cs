using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NewLife.JT1078.Client.Models;

namespace NewLife.JT1078.Client.ViewModels;

/// <summary>主窗口 ViewModel</summary>
public partial class MainWindowViewModel : ObservableObject
{
    #region 属性
    /// <summary>当前选中的 Tab 索引</summary>
    [ObservableProperty]
    private Int32 _selectedTab;

    /// <summary>连接配置</summary>
    public ConnectionConfig Config { get; } = new();

    /// <summary>Tab 页面列表</summary>
    public String[] Tabs { get; } =
    [
        "直播拉流",
        "录像回放",
        "设备命令",
        "TCP 直连",
        "UDP 直连",
        "状态监控",
    ];

    /// <summary>子 ViewModel</summary>
    public LiveStreamViewModel LiveStreamVM { get; }
    public RecordViewModel RecordVM { get; }
    public CommandViewModel CommandVM { get; }
    public TcpTestViewModel TcpTestVM { get; }
    public UdpTestViewModel UdpTestVM { get; }
    public StatusViewModel StatusVM { get; }
    #endregion

    /// <summary>实例化主窗口 ViewModel</summary>
    public MainWindowViewModel()
    {
        LiveStreamVM = new LiveStreamViewModel(Config);
        RecordVM = new RecordViewModel(Config);
        CommandVM = new CommandViewModel(Config);
        TcpTestVM = new TcpTestViewModel(Config);
        UdpTestVM = new UdpTestViewModel(Config);
        StatusVM = new StatusViewModel(Config);
    }

    /// <summary>选择 Tab</summary>
    [RelayCommand]
    private void SelectTab(Int32 index)
    {
        if (index >= 0 && index < Tabs.Length)
            SelectedTab = index;
    }
}
