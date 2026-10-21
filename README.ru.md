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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

Реализация протокола видеосвязи JT/T 1078 для систем спутникового позиционирования транспортных средств на .NET. Покрывает полный конвейер: разбор протокола, аудио/видеокодеки, ремуксинг FLV/fMP4, серверы TCP/UDP и потоковую передачу HTTP/WebSocket. Поддерживаются NativeAOT и локализация во время выполнения.

## Возможности

### Протокол
- **Протокол RTP JT1078**: полный разбор и построение сообщений RTP с 29 типами аудио/видео полезной нагрузки
- **Кадрирование**: разделение склеенных пакетов и разбор header-first на основе `IMessageCodec` из NewLife.Core
- **Объединение фрагментов RTP**: автоматическая буферизация и сборка фрагментов I/P/B кадров

### Аудио- и видеокодеки
- **Декодирование аудио**: G.711A (A-law), G.711U (μ-law), IMA-ADPCM с удалением заголовка HiSilicon
- **Кодирование AAC**: кодирование AAC через FaacEncoder (P/Invoke)
- **Разбор H.264**: разделение NAL-блоков, разбор SPS/PPS, обработка стартовых кодов и удаление байтов предотвращения эмуляции

### Ремуксинг контейнеров
- **Ремуксинг FLV**: RTP в HTTP-FLV / WebSocket-FLV в реальном времени для воспроизведения через flv.js
- **Ремуксинг fMP4**: вывод сегментов CMAF/fMP4 с основными боксами ISO BMFF

### Сервер
- **Служба TCP**: готовый сервер JT1078 TCP с настройкой порта и отладочным журналированием
- **Служба UDP**: транспорт UDP JT/T 1078, работающий параллельно с TCP
- **Трансляция HTTP/WS**: встроенные конечные точки ASP.NET Core для FLV/fMP4/AAC/PCM/WAV
- **Воспроизведение записей**: воспроизведение архивного аудио/видео по SIM, каналу и диапазону времени
- **Команды устройствам**: кластерная шина команд на основе IEventBus, автономный режим или кластер Redis

### Управление и интеграция
- **Присутствие устройств**: отслеживание событий подключения и отключения терминалов в реальном времени
- **Авторизация потоков**: аутентификация по токену, защищающая конечные точки трансляции
- **DI-построитель**: свободная регистрация `AddJT1078Gateway().AddTcp().AddHttp()`
- **IHostedService**: автоматическое управление жизненным циклом ASP.NET Core
- **NativeAOT**: протокол и таблицы локализации без рефлексии; нативный образ проверяется дымовым тестом
- **Локализация**: исходные сообщения на английском, 10 встроенных переводов и откат на английский

## Быстрый старт

### Установка пакетов NuGet

```bash
dotnet add package NewLife.JT1078           # библиотека протокола (включая FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # сервер TCP/UDP + HLS/MP3
```

### Разбор сообщения RTP

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// Кадрирование: сообщение создаётся сразу после прихода заголовка, тело привязывает кадровый слой
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // Проверяем тип полезной нагрузки
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### Декодирование аудио

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// Полезная нагрузка устройства -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### Запуск сервера трансляции

```csharp
using NewLife.JT1078.Server;

// Вариант 1: прямое использование
var server = new JT1078Server();
server.Start();

// Вариант 2: интеграция с ASP.NET Core
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## Пакеты NuGet

| Пакет | Описание | NuGet |
|---|---|---|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Основной протокол + кодеки + ремуксинг FLV/fMP4 (Flv/FMp4 объединены с ядром) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | Сервер TCP/UDP + DI-построитель + кодирование HLS/MP3 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

Основная библиотека протокола и сервер в конфигурации по умолчанию поддерживают NativeAOT и тримминг. Для целевых платформ net8.0 и выше анализаторы AOT/тримминга включены постоянно, при сборке нет ни одного IL-предупреждения. Проект `AotSmoke` в этом репозитории публикует нативный образ в CI с `PublishAot=true` и проверяет кодеки кадров, сериализацию моделей через рефлексию, сборку DI, сборку сервера и поиск в таблицах локализации.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> Плагины, динамические скрипты и регистрация через рефлексию во время выполнения выходят за границы поддержки AOT (как и в NewLife.Core).

## Локализация

Исключения и журналы используют общую таблицу локализации, создаваемую при компиляции. По умолчанию она следует `CultureInfo.CurrentUICulture`, а при отсутствии перевода возвращается к английскому. Без resx и сателлитных сборок: одна DLL, изначально безопасно для AOT.

| Параметр | Назначение |
|---|---|
| `CultureInfo.CurrentUICulture` | Источник языка по умолчанию; ASP.NET Core может менять его для каждого запроса |
| `Localization.Culture` | Принудительно задаёт язык исключений и обычного текста |
| `Localization.LogCulture` | Язык текста для разработчиков (журналы, комментарии конфигурации) |

```csharp
// Весь сайт на русском
Localization.Culture = new CultureInfo("ru");

// Веб-служба, учитывающая Accept-Language для каждого запроса
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Встроенные переводы: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar` (10 встроенных переводов). Чтобы добавить язык, сопоставьте английские исходные строки с переводами в `Locales/<culture>.yml`; генератор исходного кода проверяет, сжимает и регистрирует таблицу при сборке.

## Сборка и тесты

```bash
dotnet test -c Release
```

Тесты охватывают кадровый кодек, многосегментные последовательности, некорректные кадры, объединение фрагментов, BCD, аудиокодеки, H.264, FLV/fMP4, протокольный режим сервера и таблицы языков.

## Архитектура

```
JT1078-устройство (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  объединение фрагментов RTP
    ▼
JTMessage (сообщение RTP)
    ├──► H264Decoder (видео)
    ├──► AudioCodecFactory (аудио)
    │
    ├──► FlvEncoder ────────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ───────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ────────────► аудио MP3
    ├──► обработчики аудио ─────► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► широковещательная рассылка команд кластера (IEventBus)
    │
    ▼
Проигрыватель (flv.js / hls.js)
```

## Кластерное развёртывание

Сервер поддерживает развёртывание в несколько экземпляров и пересылает команды устройствам между экземплярами через шину событий:

```csharp
// Автономный режим (по умолчанию, EventBus в памяти)
services.AddJT1078Gateway().AddTcp().AddHttp();

// Кластер (предоставьте ICacheProvider через Redis/Stardust)
// EventBus Redis создаётся автоматически, команды рассылаются всем экземплярам
```

REST API:
- `POST /Command/Send`: синхронная отправка команды устройству с автоматической маршрутизацией на экземпляр, удерживающий устройство
- `POST /Command/SendAsync`: асинхронная отправка команды с поддержкой CancellationToken
- `GET /Command/Devices`: список устройств, подключённых к текущему экземпляру (TCP/UDP)

## Конечные точки трансляций

| Конечная точка | Формат | Транспорт | Проигрыватель |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | нативный MSE |
| `/live.aac` | AAC | HTTP | нативное аудио |
| `/live.pcm` | PCM | HTTP | нативное аудио |
| `/live.wav` | WAV | HTTP | нативное аудио |
| `/live.mp3` | MP3 | HTTP | нативное аудио |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## Ссылки на протоколы

- JT/T 1078-2016, Системы спутникового позиционирования транспортных средств — протокол видеосвязи
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## Благодарности

Дизайн и реализация этого проекта опираются на проект с открытым исходным кодом [SmallChi/JT1078](https://github.com/SmallChi/JT1078). Большое спасибо.

## Карта экосистемы

NewLife — это многоуровневая экосистема независимо версионируемых open-source проектов. Карта ниже дословно используется во всех репозиториях NewLife; каждый проект ссылается на свой репозиторий и свой основной пакет NuGet.

| Проект | Что делает | Загрузки |
| --- | --- | --- |
| **Базовый** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Логирование, конфигурация, кэширование, сеть, сериализация, планирование и APM-трассировка. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Промежуточное ПО данных: отдельные таблицы масштаба десятков миллиардов строк, автоматическое секционирование, разделение чтения и записи, более 10 баз данных. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Высокопроизводительная сетевая библиотека: 1,77M pps / 14,5 Гбит/с на машину (TCP 1KB, 2026-10), 4M долгоживущих TCP-соединений. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Протокольная связь и RPC-фреймворк для клиент-серверных приложений и IoT-устройств. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Устанавливает .NET-приложения как демоны ОС: службы Windows и systemd в Linux. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | Шаблоны проектов `dotnet new` для консоли, web, API, службы, данных и IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Данные и обмен сообщениями** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Клиент Redis с микросекундной задержкой, миллионами операций в секунду и встроенными очередями сообщений. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Полностью управляемый клиент RocketMQ для Apache RocketMQ и Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | Клиент и сервер MQTT, включая подключение к Alibaba Cloud IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Гибридная база данных на C#: встроенная или сетевая, реляционная, временных рядов, очередь сообщений и KV в одном. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | Драйвер MySql и вспомогательный слой на основе стека данных NewLife. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, AI и распределённые системы** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Фреймворк быстрой разработки админ-платформ: права, SSO, сервер OAuth и CRUD-скаффолдинг. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Распределённая сервисная платформа: управление узлами, APM, центр конфигурации, реестр и центр публикации. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Распределённая платформа задач и планирования данных в реальном времени с секционированием, повторами и дозаполнением. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Единый клиент для 46 поставщиков LLM плюс ChatAI — готовое веб-приложение для чата. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Промышленность и IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | Стандартная библиотека IoT, определяющая контракты протоколов и абстракции устройств. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Реализация Modbus TCP/RTU/ASCII поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Протокол Siemens PLC поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Протокол Omron PLC поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Протокол Mitsubishi Melsec PLC поверх NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | Клиент OPC для доступа к промышленным данным. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | Стек протокола автоматизации зданий BACnet. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Лёгкая open-source платформа IoT с управлением шлюзами и устройствами. | — |
| **Телематика транспорта** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | Протокол позиционирования транспорта JT/T 808: более 70 типов сообщений, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | Межплатформенный протокол JT/T 809 для органов управления транспортом. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Автомобильное видео JT/T 1078: разбор RTP, ремультиплексирование в FLV/fMP4 и прямая трансляция. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Прикладные компоненты** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Один пакет для Excel, Word, PPT, PDF и более 15 форматов документов. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Кроссплатформенное аудио: кодеки, ввод-вывод устройств, DSP, контейнеры и потоковая передача. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Видеокодеки, форматы контейнеров, обработка кадров и потоковая передача. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Единый интерфейс к Baidu, AMap, Tencent и Tianditu с геокодированием и преобразованием координат. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | Геолокация IPv4 с офлайн-базой, отображаемой в память, и онлайн-API как резервом. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Инструменты и продукты** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Инструменты разработчика: сеть, последовательный порт, шифрование, регулярные выражения, Modbus и MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Самостоятельно размещаемая система управления исходным кодом Git. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Автоматически устанавливает подходящую среду выполнения .NET при запуске на старом хосте. | — |

> Бейджи показывают основной пакет каждого проекта; пакеты тем, расширений и сателлитов перечислены на соответствующих страницах NuGet. Более 30 пакетов, не перечисленных здесь (FTP, LoRa, расширения Office, драйверы протоколов и другие), доступны в [организации NewLifeX](https://github.com/NewLifeX) и на [newlifex.com](https://newlifex.com).

## Команда

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

Команда NewLife основана в 2002 году и более двух десятилетий работает в экосистеме .NET и IoT, поддерживает около 100 проектов с открытым кодом (34 из них — основные) и предлагает интегрированные программно-аппаратные решения для энергетики, логистики, здравоохранения, промышленной автоматизации и интернет-отрасли. Мы следуем девизу «в учении нет старших — мастерство приносит уважение».

Open-source компоненты скачали <!--nuget-total:start-->6.48M<!--nuget-total:end--> раз с NuGet (по всей экосистеме, по состоянию на 2026-10).

Сайт: https://newlifex.com · Организация open source: https://github.com/NewLifeX

<details>
<summary>Официальный аккаунт WeChat: 智能大石头 (отсканируйте, чтобы подписаться)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## Лицензия

[MIT](LICENSE) © 2022-2026 NewLife
