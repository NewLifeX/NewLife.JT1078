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

- **Протокол RTP JT1078**: полный разбор и построение сообщений, 29 типов аудио/видео
- **Кадрирование**: разделение склеенных пакетов и разбор header-first на основе `IMessageCodec` из NewLife.Core
- **Объединение фрагментов RTP**: буферизация и сборка фрагментов I/P/B
- **Декодирование аудио**: G.711A, G.711U, IMA-ADPCM с удалением заголовка HiSilicon
- **Разбор H.264**: разделение NAL, SPS/PPS, стартовые коды и байты предотвращения эмуляции
- **Ремуксинг**: FLV и fMP4 в реальном времени, вывод HTTP-FLV/WS-FLV, fMP4 и HLS
- **Сервер**: TCP/UDP, конечные точки HTTP/WS, воспроизведение записей и кластерная шина команд (IEventBus)
- **NativeAOT**: протокол и таблицы языков без рефлексии; нативный образ проверяется в CI
- **Локализация**: исходные сообщения на английском, 10 переводов, откат на английский

## Быстрый старт

```bash
dotnet add package NewLife.JT1078           # библиотека протокола (включая FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # сервер TCP/UDP + HLS/MP3
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

// Или интеграция с ASP.NET Core
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## Пакеты NuGet

| Пакет | Описание |
|---|---|
| NewLife.JT1078 | Протокол + кодеки + ремуксинг FLV/fMP4 |
| NewLife.JT1078.Server | Сервер TCP/UDP + DI-построитель + HLS/MP3 |

## Native AOT

Библиотека протокола и сервер в конфигурации по умолчанию поддерживают NativeAOT и тримминг. Для net8.0 и выше анализаторы AOT/тримминга включены постоянно, IL-предупреждений нет. Проект `AotSmoke` публикует нативный образ в CI с `PublishAot=true` и проверяет кодеки кадров и таблицы языков.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## Локализация

Исключения и журналы используют общую таблицу, создаваемую при компиляции. По умолчанию применяется `CultureInfo.CurrentUICulture`, при отсутствии перевода — английский. Без resx и сателлитных сборок: одна DLL, безопасно для AOT.

```csharp
Localization.Culture = new CultureInfo("ru");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Включённые языки: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. Новый язык добавляется в `Locales/<culture>.yml`.

## Сборка и тесты

```bash
dotnet test -c Release
```

Тесты охватывают кадровый кодек, многосегментные последовательности, некорректные кадры, объединение фрагментов, BCD, аудиокодеки, H.264, FLV/fMP4, протокольный режим сервера и таблицы языков.

## Экосистема

Полная матрица проектов — в [README.md](README.md#ecosystem) (одинакова во всех репозиториях NewLife).

## Лицензия

[MIT](LICENSE) © 2022-2026 NewLife
