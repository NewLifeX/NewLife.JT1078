using Avalonia.Controls;
using NewLife.JT1078.Client.ViewModels;

namespace NewLife.JT1078.Client.Views;

/// <summary>主窗口</summary>
public partial class MainWindow : Window
{
    /// <summary>实例化主窗口</summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }
}
