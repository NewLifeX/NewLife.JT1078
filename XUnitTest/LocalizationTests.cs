using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using NewLife;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;
using NewLife.Log;
using Xunit;

namespace XUnitTest;

/// <summary>多语言本地化测试。覆盖异常消息、日志消息、程序集元数据与回退规则</summary>
/// <remarks>用例会临时改写 <see cref="Localization.Culture"/> 与 <see cref="Localization.LogCulture"/>，归入串行集合避免并行干扰</remarks>
[Collection("GlobalState")]
public class LocalizationTests
{
    static LocalizationTests()
    {
        // 强制运行服务端程序集的模块初始化器，完成其词表注册；仅写 typeof(T) 可能被 JIT 优化掉
        RuntimeHelpers.RunModuleConstructor(typeof(JT1078Server).Assembly.ManifestModule.ModuleHandle);
    }

    private static readonly String[] _languages =
    [
        "zh-CN", "zh-TW", "de-DE", "es-ES", "fr-FR", "ja-JP", "ko-KR", "pt-BR", "ru-RU", "ar-SA"
    ];

    private static void WithCulture(String name, Action action)
    {
        var prev = Localization.Culture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo(name);
            action();
        }
        finally
        {
            Localization.Culture = prev;
        }
    }

    [Fact(DisplayName = "异常消息：按界面语言翻译，未支持语言回退英文")]
    public void ExceptionMessage()
    {
        const String key = "The message must be JTMessage.";

        WithCulture("zh-CN", () => Assert.Equal("消息必须是 JTMessage 类型。", Localization.Translate(key)));
        WithCulture("de-DE", () => Assert.Equal("Die Nachricht muss vom Typ JTMessage sein.", Localization.Translate(key)));
        WithCulture("sw-KE", () => Assert.Equal(key, Localization.Translate(key)));
    }

    [Fact(DisplayName = "日志消息：按日志语言翻译，与界面语言相互独立")]
    public void LogMessage()
    {
        var prevCulture = Localization.Culture;
        var prevLogCulture = Localization.LogCulture;
        try
        {
            Localization.Culture = CultureInfo.GetCultureInfo("en-US");
            Localization.LogCulture = CultureInfo.GetCultureInfo("ja-JP");

            Assert.Equal("JT1078 server started on port 1078", Localization.Translate("JT1078 server started on port {0}", 1078));
            Assert.Equal("JT1078 サーバーを起動しました。ポート 1078", Localization.TranslateForLog("JT1078 server started on port {0}", 1078));
        }
        finally
        {
            Localization.Culture = prevCulture;
            Localization.LogCulture = prevLogCulture;
        }
    }

    [Fact(DisplayName = "词表：10 种语言均提供消息译文")]
    public void AllLanguages()
    {
        const String key = "JT/T 1078 video streaming server built on NewLife.Net: RTP fragment merging, device session management, live endpoints, record playback and cluster command bus.";

        foreach (var name in _languages)
        {
            WithCulture(name, () => Assert.NotEqual(key, Localization.Translate(key)));
        }
    }

    [Fact(DisplayName = "日志链路：InfoPrefixed 按日志语言翻译并保留前缀")]
    public void LogPipeline()
    {
        var prev = Localization.LogCulture;
        var logger = new MemoryLogger();
        try
        {
            Localization.LogCulture = CultureInfo.GetCultureInfo("zh-Hans");

            logger.Info("JT1078 server started on port {0}", 1078);
            logger.InfoPrefixed("DeviceSessionManager", "Register device session {0}", "SIM-1");

            Assert.Contains("服务端已启动", logger.Messages[0]);
            Assert.Contains("1078", logger.Messages[0]);
            Assert.Contains("DeviceSessionManager", logger.Messages[1]);
            Assert.Contains("注册设备会话", logger.Messages[1]);
        }
        finally
        {
            Localization.LogCulture = prev;
        }
    }

    private sealed class MemoryLogger : Logger
    {
        public List<String> Messages { get; } = [];

        protected override void OnWrite(LogLevel level, String format, params Object?[] args) => Messages.Add(Format(format, args));
    }

    [Fact(DisplayName = "占位符：全部语言格式化不抛异常且保留参数")]
    public void Placeholders()
    {
        const String key = "HLS segment completed seq={0} size={1}KB duration={2:F2}s";

        foreach (var name in _languages)
        {
            WithCulture(name, () =>
            {
                var text = Localization.Translate(key, 3, 128, 2.5);
                Assert.Contains("3", text);
                Assert.Contains("128", text);
                Assert.Contains("2", text);
            });
        }
    }

    [Fact(DisplayName = "程序集元数据：标题为英文源文，可按目标语言翻译")]
    public void AssemblyTitle()
    {
        var coreTitle = typeof(JTMessage).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        var serverTitle = typeof(JT1078Server).Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;

        Assert.Equal("NewLife JT1078 Protocol", coreTitle);
        Assert.Equal("NewLife JT1078 Server", serverTitle);

        WithCulture("zh-CN", () =>
        {
            Assert.Equal("NewLife JT1078 协议核心", Localization.Translate(coreTitle!));
            Assert.Equal("NewLife JT1078 服务端", Localization.Translate(serverTitle!));
        });
    }
}
