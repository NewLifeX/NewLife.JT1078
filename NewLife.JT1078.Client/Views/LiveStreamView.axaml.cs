using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using NewLife.JT1078.Client.Services;
using NewLife.JT1078.Client.ViewModels;

namespace NewLife.JT1078.Client.Views;

/// <summary>直播拉流页面</summary>
public partial class LiveStreamView : UserControl
{
    private readonly VlcPlayerService _player = new();
    private LiveStreamViewModel? _viewModel;
    private String? _pendingUrl;

    /// <summary>实例化直播拉流页面</summary>
    public LiveStreamView()
    {
        InitializeComponent();

        _player.ErrorOccurred += OnPlayerError;
        DataContextChanged += OnDataContextChanged;
        LayoutUpdated += OnLayoutUpdated;
        PropertyChanged += OnPropertyChanged;
        Unloaded += OnUnloaded;
    }

    private void OnDataContextChanged(Object? sender, EventArgs e)
    {
        if (_viewModel != null) _viewModel.PropertyChanged -= OnViewModelPropertyChanged;

        _viewModel = DataContext as LiveStreamViewModel;
        if (_viewModel != null) _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(Object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LiveStreamViewModel.PlaybackUrl)) UpdatePlayback();
    }

    private void UpdatePlayback()
    {
        var url = _viewModel?.PlaybackUrl;
        if (String.IsNullOrEmpty(url))
        {
            _pendingUrl = null;
            _player.Stop();
            return;
        }

        if (!_player.EnsureInitialized())
        {
            _viewModel!.ReportPlayerError(_player.LastError ?? "播放器初始化失败");
            return;
        }

        Video.Player = _player.Player;
        _pendingUrl = url;

        // 原生窗口由布局阶段创建，未就绪时等下一轮布局再开始播放
        TryStartPlayback();
        if (_pendingUrl != null) Dispatcher.UIThread.Post(TryStartPlayback, DispatcherPriority.Loaded);
    }

    private void TryStartPlayback()
    {
        if (_pendingUrl == null || !Video.IsHandleReady) return;

        var url = _pendingUrl;
        _pendingUrl = null;

        // 再次绑定，确保新建的原生窗口接到播放器
        Video.Player = _player.Player;
        if (!_player.Play(url)) _viewModel?.ReportPlayerError(_player.LastError ?? "播放器启动失败");
    }

    private void OnLayoutUpdated(Object? sender, EventArgs e) => TryStartPlayback();

    private void OnPlayerError(Object? sender, String message) => Dispatcher.UIThread.Post(() => _viewModel?.ReportPlayerError(message));

    private void OnPropertyChanged(Object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        // 切到其他页面时停止播放，避免原生视频窗口覆盖其他页面
        if (e.Property.Name == nameof(IsEffectivelyVisible) && !IsEffectivelyVisible) _viewModel?.StopPlayback();
    }

    private void OnUnloaded(Object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _viewModel?.StopPlayback();
        _player.Stop();
    }
}
