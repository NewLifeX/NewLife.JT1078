using System.Globalization;
using System.Runtime.CompilerServices;
using NewLife;
using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Protocols;

/// <summary>运行时多语言本地化测试。语言为进程级全局状态，禁止并行</summary>
[Collection("GlobalState")]
public class LocalizationTests
{
    [Fact(DisplayName = "内置语言表已注册")]
    public void SupportedCultures()
    {
        foreach (var culture in new[] { "zh-Hans", "zh-Hant", "es", "de", "ja", "fr", "pt", "ru", "ko", "ar" })
        {
            Assert.True(Localization.IsSupported(new CultureInfo(culture)), culture);
        }
    }

    [Fact(DisplayName = "异常消息按当前语言翻译")]
    public void Exception_Translated()
    {
        var old = Localization.Culture;
        try
        {
            var codec = new JTCodec();

            Localization.Culture = new CultureInfo("zh-Hans");
            var zh = Assert.Throws<ArgumentException>(() => codec.Build(null!));
            Assert.Contains("JTMessage", zh.Message);
            Assert.Contains("消息必须", zh.Message);

            Localization.Culture = new CultureInfo("en-US");
            var en = Assert.Throws<ArgumentException>(() => codec.Build(null!));
            Assert.Equal("The message must be JTMessage.", en.Message.Split(" (")[0]);
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact(DisplayName = "缺少译文时回退英文原文")]
    public void MissingTranslation_FallsBack()
    {
        var old = Localization.Culture;
        try
        {
            Localization.Culture = new CultureInfo("zh-Hans");
            Assert.Equal("Unknown message without translation", Localization.Translate("Unknown message without translation"));
        }
        finally
        {
            Localization.Culture = old;
        }
    }

    [Fact(DisplayName = "服务端日志词条按语言翻译")]
    public void ServerLog_Translated()
    {
        // 强制运行服务端程序集的模块初始化器，完成词表注册；仅写 typeof(T) 可能被 JIT 优化掉
        RuntimeHelpers.RunModuleConstructor(typeof(NewLife.JT1078.Server.JT1078Server).Assembly.ManifestModule.ModuleHandle);

        var old = Localization.Culture;
        try
        {
            Localization.Culture = new CultureInfo("zh-Hans");
            var text = Localization.Translate("Server started on port {0}", 1078);
            Assert.Contains("服务端已启动", text);
            Assert.Contains("1078", text);
        }
        finally
        {
            Localization.Culture = old;
        }
    }
}
