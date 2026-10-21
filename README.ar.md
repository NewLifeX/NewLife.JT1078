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

تنفيذ .NET لبروتوكول اتصال الفيديو JT/T 1078 لأنظمة تحديد المواقع بالأقمار الصناعية لمركبات النقل البري. تغطي المكتبة سلسلة كاملة من تحليل البروتوكول وترميز الصوت والفيديو وإعادة تغليف FLV/fMP4 وخوادم TCP/UDP والبث المباشر عبر HTTP/WebSocket. يدعم NativeAOT والتوطين أثناء التشغيل.

## الميزات

### البروتوكول
- **بروتوكول RTP JT1078**: تحليل وبناء كامل لرسائل RTP مع 29 نوعًا من حمولات الصوت والفيديو
- **تحديد الإطارات**: تقسيم الحزم الملتصقة وتحليل الرأس أولًا باستخدام `IMessageCodec` من NewLife.Core
- **دمج شظايا RTP**: تخزين ودمج شظايا إطارات I/P/B تلقائيًا

### ترميز وفك ترميز الصوت والفيديو
- **فك ترميز الصوت**: G.711A (A-law) وG.711U (μ-law) وIMA-ADPCM مع إزالة رأس HiSilicon
- **ترميز AAC**: ترميز AAC عبر FaacEncoder (P/Invoke)
- **تحليل H.264**: تقسيم NAL وSPS/PPS ورموز البداية وإزالة بايتات منع المحاكاة

### إعادة تغليف الحاويات
- **إعادة تغليف FLV**: تحويل RTP في الوقت الحقيقي إلى HTTP-FLV / WebSocket-FLV للتشغيل بواسطة flv.js
- **إعادة تغليف fMP4**: إخراج مقاطع CMAF/fMP4 مع صناديق ISO BMFF الأساسية

### الخادم
- **خدمة TCP**: خادم JT1078 TCP جاهز للاستخدام مع تهيئة المنفذ وتسجيل التصحيح
- **خدمة UDP**: نقل JT/T 1078 عبر UDP بالتوازي مع TCP
- **بث HTTP/WS المباشر**: نقاط نهاية ASP.NET Core مدمجة بصيغ FLV/fMP4/AAC/PCM/WAV
- **إعادة التشغيل**: تشغيل الصوت والفيديو التاريخي حسب SIM والقناة والنطاق الزمني
- **أوامر الأجهزة**: ناقل أوامر عنقودي مبني على IEventBus، بوضع مستقل أو عنقود Redis

### الإدارة والتكامل
- **حضور الأجهزة**: تتبّع فوري لأحداث اتصال الأجهزة الطرفية وانقطاعها
- **ترخيص البث**: مصادقة بالرمز (Token) لحماية نقاط النهاية المباشرة
- **منشئ DI**: تسجيل سلس عبر `AddJT1078Gateway().AddTcp().AddHttp()`
- **IHostedService**: إدارة تلقائية لدورة حياة ASP.NET Core
- **NativeAOT**: بروتوكول وجداول لغات بدون انعكاس؛ صورة أصلية يتم التحقق منها في CI
- **التوطين**: رسائل مصدرها الإنجليزية مع 10 ترجمات مدمجة وتراجع إلى الإنجليزية

## البدء السريع

### تثبيت حزم NuGet

```bash
dotnet add package NewLife.JT1078           # مكتبة البروتوكول الأساسية (تتضمن إعادة تغليف FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # خادم TCP/UDP + HLS/MP3
```

### تحليل رسالة RTP

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// تحديد الإطارات: تُنتَج الرسالة بمجرد وصول الرأس، ويتولى الطبقة الإطارية ربط الحمولة
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // فحص نوع الحمولة
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### فك ترميز الصوت

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// حمولة الجهاز -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### تشغيل خادم البث

```csharp
using NewLife.JT1078.Server;

// الخيار 1: الاستخدام المباشر
var server = new JT1078Server();
server.Start();

// الخيار 2: التكامل مع ASP.NET Core
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## حزم NuGet

| الحزمة | الوصف | NuGet |
|---------|-------------|-------|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | البروتوكول الأساسي + برامج الترميز + إعادة تغليف FLV/fMP4 (دمج Flv/FMp4 في النواة) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | خادم TCP/UDP + منشئ DI + ترميز HLS/MP3 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

تدعم مكتبة البروتوكول والخادم بالإعداد الافتراضي NativeAOT والتقليم. محللات AOT/التقليم مفعلة دائمًا لإصدارات net8.0 وما بعده دون تحذيرات IL. ينشر مشروع `AotSmoke` في هذا المستودع صورة أصلية في CI باستخدام `PublishAot=true`، ويختبر ترميز الإطارات، وتسلسل انعكاس النماذج، وتجميع DI، وتجميع الخادم، والبحث في جداول اللغات.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> الإضافات والبرمجة النصية الديناميكية وتسجيل الانعكاس أثناء التشغيل خارج نطاق دعم AOT (بما يتوافق مع حدود NewLife.Core).

## التوطين

تستخدم الاستثناءات والسجلات جدولًا مشتركًا يُنشأ وقت الترجمة. يتبع `CultureInfo.CurrentUICulture` افتراضيًا ويعود إلى الإنجليزية عند غياب الترجمة. بدون resx أو تجميعات تابعة: ملف DLL واحد وآمن مع AOT.

| الإعداد | الغرض |
|---|---|
| `CultureInfo.CurrentUICulture` | مصدر اللغة الافتراضي؛ ويستطيع ASP.NET Core تغييره لكل طلب |
| `Localization.Culture` | يفرض لغة الاستثناءات والنصوص العادية |
| `Localization.LogCulture` | لغة النصوص الموجّهة للمطوّر (السجلات وتعليقات الإعدادات) |

```csharp
// الموقع بالكامل بالعربية
Localization.Culture = new CultureInfo("ar");

// خدمة ويب تحترم Accept-Language لكل طلب
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

الترجمات المضمنة: `zh-Hans` و`zh-Hant` و`es` و`de` و`ja` و`fr` و`pt` و`ru` و`ko` و`ar` — 10 ترجمات مضمنة. لإضافة لغة، عيّن النصوص الإنجليزية المصدر إلى الترجمات في `Locales/<culture>.yml`؛ ويتحقق المولّد المصدري من الجدول ويضغطه ويسجّله وقت البناء.

## البناء والاختبار

```bash
dotnet test -c Release
```

تغطي الاختبارات ترميز الإطارات والتسلسلات متعددة المقاطع والإطارات التالفة ودمج الشظايا وBCD وبرامج ترميز الصوت وH.264 وFLV/fMP4 ووضع البروتوكول في الخادم وجداول اللغات.

## البنية المعمارية

```
JT1078 device (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  RTP fragment merging
    ▼
JTMessage (RTP message)
    ├──► H264Decoder (video)
    ├──► AudioCodecFactory (audio)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► MP3 audio
    ├──► audio handlers ──────► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► cluster command broadcast (IEventBus)
    │
    ▼
Player (flv.js / hls.js)
```

## النشر العنقودي

يدعم الخادم النشر متعدد النسخ، ويمرّر أوامر الأجهزة بين النسخ عبر ناقل أحداث:

```csharp
// مستقل (الافتراضي، EventBus في الذاكرة)
services.AddJT1078Gateway().AddTcp().AddHttp();

// عنقودي (وفّر ICacheProvider عبر Redis/Stardust)
// يُنشأ EventBus من نوع Redis تلقائيًا وتُبثّ الأوامر إلى جميع النسخ
```

واجهة REST:
- `POST /Command/Send`: إرسال أمر إلى جهاز بشكل متزامن، مع التوجيه التلقائي إلى النسخة التي تحتفظ بالجهاز
- `POST /Command/SendAsync`: إرسال أمر بشكل غير متزامن مع دعم CancellationToken
- `GET /Command/Devices`: عرض الأجهزة المتصلة محليًا (TCP/UDP)

## نقاط النهاية المباشرة

| نقطة النهاية | الصيغة | النقل | المشغّل |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | MSE الأصلية |
| `/live.aac` | AAC | HTTP | صوت أصلي |
| `/live.pcm` | PCM | HTTP | صوت أصلي |
| `/live.wav` | WAV | HTTP | صوت أصلي |
| `/live.mp3` | MP3 | HTTP | صوت أصلي |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## مراجع البروتوكول

- `JT/T 1078-2016`: نظام تحديد المواقع بالأقمار الصناعية لمركبات النقل البري — بروتوكول اتصال الفيديو
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## شكر وتقدير

يعتمد تصميم هذا المشروع وتنفيذه على المشروع مفتوح المصدر [SmallChi/JT1078](https://github.com/SmallChi/JT1078). شكرًا جزيلًا.

## خريطة المنظومة

NewLife منظومة متعدّدة الطبقات من مشاريع مفتوحة المصدر تُصدَر بإصدارات مستقلة. الخريطة أدناه مشتركة حرفيًا بين جميع مستودعات NewLife؛ وكل مشروع يرتبط بمستودعه وبحزمته الرئيسية على NuGet.

| المشروع | الوظيفة | التنزيلات |
| --- | --- | --- |
| **الأساس** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | التسجيل والتهيئة والتخزين المؤقت والشبكات والتسلسل والجدولة وتتبّع أداء APM. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | وسيط بيانات: جداول واحدة بمستوى عشرات المليارات من الصفوف، تقسيم تلقائي للجداول، فصل القراءة عن الكتابة، وأكثر من 10 قواعد بيانات. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | مكتبة شبكات عالية الأداء: 1.77 مليون pps / 14.5 Gbps على جهاز واحد (TCP 1KB، 2026-10)، و4 ملايين اتصال TCP طويل الأمد. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | إطار اتصال بروتوكولي وRPC لتطبيقات العميل/الخادم وأجهزة إنترنت الأشياء. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | يثبّت تطبيقات .NET كخدمات حارسة لنظام التشغيل: خدمات Windows وsystemd في Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | قوالب مشاريع `dotnet new` للطرفية وweb وAPI والخدمات والبيانات وإنترنت الأشياء. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **البيانات والمراسلة** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | عميل Redis بزمن استجابة بالميكروثانية، وملايين العمليات في الثانية، وقوائم رسائل مدمجة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | عميل RocketMQ مُدار بالكامل لـ Apache RocketMQ وخدمة رسائل Alibaba Cloud. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | عميل وخادم MQTT، بما في ذلك الاتصال بـ Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | قاعدة بيانات هجينة بلغة C#: مدمجة أو شبكية، وعلائقية، وسلاسل زمنية، وقائمة رسائل، وKV في واحد. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | برنامج تشغيل MySql وطبقة مساعدة مبنية على مكدّس بيانات NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web والذكاء الاصطناعي والأنظمة الموزّعة** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | إطار منصة إدارة سريعة: الصلاحيات، والدخول الموحّد SSO، وخادم OAuth، وتوليد واجهات CRUD. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | منصة خدمات موزّعة: إدارة العقد، وAPM، ومركز الإعدادات، ومركز التسجيل، ومركز النشر. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | منصة موزّعة للمهام وجدولة البيانات الفورية، مع التقسيم وإعادة المحاولة وإعادة التعبئة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | عميل موحّد لـ 46 مزوّد LLM بالإضافة إلى ChatAI، تطبيق ويب جاهز للمحادثة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **الصناعة وإنترنت الأشياء** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | مكتبة معايير إنترنت الأشياء التي تعرّف عقود البروتوكولات وتجريدات الأجهزة. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | تنفيذ Modbus TCP/RTU/ASCII فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | بروتوكول Siemens PLC فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | بروتوكول Omron PLC فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | بروتوكول Mitsubishi Melsec PLC فوق NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | عميل OPC للوصول إلى البيانات الصناعية. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | مكدّس بروتوكول BACnet لأتمتة المباني. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | منصة إنترنت أشياء مفتوحة المصدر وخفيفة، مع إدارة البوابات والأجهزة. | — |
| **تليماتيكا المركبات** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | بروتوكول تحديد مواقع المركبات JT/T 808: أكثر من 70 جسم رسالة، 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | بروتوكول JT/T 809 بين المنصات لجهات إدارة النقل. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | فيديو داخل المركبة JT/T 1078: تحليل RTP وإعادة التجميع إلى FLV/fMP4 والبث المباشر. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **مكوّنات المجال** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | حزمة واحدة لـ Excel وWord وPPT وPDF وأكثر من 15 صيغة مستندات. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | صوت عبر المنصات: مرمّزات، وإدخال/إخراج الأجهزة، وDSP، وحاويات، وبث. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | مرمّزات الفيديو وصيغ الحاويات ومعالجة الإطارات والبث. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | واجهة موحّدة فوق Baidu وAMap وTencent وTianditu مع الترميز الجغرافي وتحويل الإحداثيات. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | تحديد الموقع الجغرافي لعناوين IPv4 بقاعدة بيانات غير متصلة مُعيّنة في الذاكرة، وواجهة API عبر الإنترنت كبديل. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **الأدوات والمنتجات** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | صندوق أدوات المطوّر: الشبكات والمنفذ التسلسلي والتشفير والتعبيرات النمطية وModbus وMQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | نظام استضافة ذاتية لإدارة الشيفرة المصدرية Git. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | يثبّت بيئة تشغيل .NET المطابقة تلقائيًا عند التشغيل من مضيف أقدم. | — |

> تعرض الشارات الحزمة الرئيسية لكل مشروع؛ أما حزم السمات والإضافات والحزم التابعة فمذكورة في صفحاتها على NuGet. الحزم التي تتجاوز 30 حزمة وغير المذكورة هنا (FTP وLoRa وإضافات Office وبرامج تشغيل البروتوكولات وغيرها) متاحة في [منظمة NewLifeX](https://github.com/NewLifeX) وعلى [newlifex.com](https://newlifex.com).

## الفريق

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

تأسّست NewLife في عام 2002، وأمضت أكثر من عقدين في منظومة .NET وإنترنت الأشياء، وتدير ما يقارب 100 مشروع مفتوح المصدر (34 منها مشاريع رئيسية)، وتقدّم حلولًا متكاملة للبرمجيات والأجهزة لقطاعات الكهرباء والخدمات اللوجستية والرعاية الصحية والتحكّم الصناعي والإنترنت. ونلتزم بمبدأ "لا أسبقية في التعلّم — الإتقان يكسب الاحترام".

نُزّلت المكوّنات مفتوحة المصدر <!--nuget-total:start-->6.48M<!--nuget-total:end--> مرة من NuGet (على مستوى المنظومة بأكملها، حتى 2026-10).

الموقع: https://newlifex.com · المنظمة مفتوحة المصدر: https://github.com/NewLifeX

<details>
<summary>حساب WeChat الرسمي: 智能大石头 (امسح للاشتراك)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## الترخيص

[MIT](LICENSE) © 2022-2026 NewLife
