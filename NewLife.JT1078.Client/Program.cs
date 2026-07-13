using Avalonia;

namespace NewLife.JT1078.Client;

/// <summary>JT1078 测试客户端入口</summary>
internal static class Program
{
    /// <summary>应用程序入口点</summary>
    [STAThread]
    private static void Main(String[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    /// <summary>构建 Avalonia 应用</summary>
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
