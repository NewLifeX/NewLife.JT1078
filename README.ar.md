# NewLife.JT1078

![GitHub License](https://img.shields.io/github/license/NewLifeX/NewLife.JT1078?logo=github)
![Nuget Downloads](https://img.shields.io/nuget/dt/NewLife.JT1078?logo=nuget)
![Nuget](https://img.shields.io/nuget/v/NewLife.JT1078?logo=nuget)
![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/NewLife.JT1078?label=dev%20nuget&logo=nuget)
[![NativeAOT](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/aot.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/aot.yml)
[![test](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/test.yml/badge.svg?branch=master)](https://github.com/NewLifeX/NewLife.JT1078/actions/workflows/test.yml)
![TFM](https://img.shields.io/badge/TFM-5%20%28net461%20%E2%86%92%20net10.0%29-512BD4?logo=dotnet)
![locales](https://img.shields.io/badge/locales-10-2ea44f?logo=dotnet)
![platform](https://img.shields.io/badge/platform-Windows%20%7C%20Linux%20%7C%20macOS-blue)

<!-- lang:start -->
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md)
<!-- lang:end -->

تنفيذ .NET لبروتوكول اتصال الفيديو JT/T 1078 لأنظمة تحديد المواقع بالأقمار الصناعية لمركبات النقل البري. يغطي المكتبة سلسلة كاملة من تحليل البروتوكول وترميز الصوت والفيديو وإعادة تغليف FLV/fMP4 وخوادم TCP/UDP والبث المباشر عبر HTTP/WebSocket. يدعم NativeAOT والتوطين أثناء التشغيل.

## الميزات

- **بروتوكول RTP JT1078**: تحليل وبناء كامل لرسائل RTP مع 29 نوعًا من حمولات الصوت والفيديو
- **تحديد الإطارات**: تقسيم الحزم الملتصقة وتحليل الرأس أولًا باستخدام `IMessageCodec` من NewLife.Core
- **دمج شظايا RTP**: تخزين ودمج شظايا إطارات I/P/B تلقائيًا
- **فك ترميز الصوت**: G.711A وG.711U وIMA-ADPCM مع إزالة رأس HiSilicon
- **تحليل H.264**: تقسيم NAL وSPS/PPS ورموز البداية وإزالة بايتات منع المحاكاة
- **إعادة التغليف**: FLV وfMP4 في الوقت الحقيقي وإخراج HTTP-FLV/WS-FLV وfMP4 وHLS
- **الخادم**: TCP/UDP ونقاط نهاية HTTP/WS وإعادة تشغيل التسجيلات وناقل أوامر عنقودي (IEventBus)
- **NativeAOT**: بروتوكول وجداول لغات بدون انعكاس؛ صورة أصلية يتم التحقق منها في CI
- **التوطين**: رسائل مصدرها الإنجليزية مع 10 ترجمات مدمجة وتراجع إلى الإنجليزية

## البدء السريع

```bash
dotnet add package NewLife.JT1078           # مكتبة البروتوكول (تتضمن FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # خادم TCP/UDP + HLS/MP3
```

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    if (msg.Type == AVTypes.H264)
    {
        var frame = new H264Decoder().ParseAVFrame(msg);
    }
}
```

```csharp
using NewLife.JT1078.Server;

var server = new JT1078Server();
server.Start();

// أو التكامل مع ASP.NET Core
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## حزم NuGet

| الحزمة | الوصف |
|---|---|
| NewLife.JT1078 | البروتوكول + برامج الترميز + إعادة تغليف FLV/fMP4 |
| NewLife.JT1078.Server | خادم TCP/UDP + منشئ DI + HLS/MP3 |

## Native AOT

تدعم مكتبة البروتوكول والخادم بالإعداد الافتراضي NativeAOT والتقليم. محللات AOT/التقليم مفعلة دائمًا لإصدارات net8.0 وما بعده دون تحذيرات IL. ينشر مشروع `AotSmoke` صورة أصلية في CI باستخدام `PublishAot=true` ويختبر ترميز الإطارات وجداول اللغات.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## التوطين

تستخدم الاستثناءات والسجلات جدولًا مشتركًا يُنشأ وقت الترجمة. يتبع `CultureInfo.CurrentUICulture` افتراضيًا ويعود إلى الإنجليزية عند غياب الترجمة. بدون resx أو تجميعات تابعة: ملف DLL واحد وآمن مع AOT.

```csharp
Localization.Culture = new CultureInfo("ar");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

اللغات المضمنة: `zh-Hans` و`zh-Hant` و`es` و`de` و`ja` و`fr` و`pt` و`ru` و`ko` و`ar`. لإضافة لغة، أضف التعيينات في `Locales/<culture>.yml`.

## البناء والاختبار

```bash
dotnet test -c Release
```

تغطي الاختبارات ترميز الإطارات والتسلسلات متعددة المقاطع والإطارات التالفة ودمج الشظايا وBCD وبرامج ترميز الصوت وH.264 وFLV/fMP4 ووضع البروتوكول في الخادم وجداول اللغات.

## المنظومة

راجع مصفوفة المشاريع الكاملة في [README.md](README.md#ecosystem) (متطابقة في جميع مستودعات NewLife).

## الترخيص

[MIT](LICENSE) © 2022-2026 NewLife
