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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

Eine .NET-Implementierung des JT/T 1078-Videokommunikationsprotokolls für Satellitenpositionierungssysteme im Straßentransport. Die Bibliothek deckt die gesamte Kette ab: Protokollanalyse, Audio-/Videocodecs, FLV/fMP4-Remuxing, TCP/UDP-Server und HTTP/WebSocket-Livestreaming. NativeAOT und Laufzeitlokalisierung werden unterstützt.

## Funktionen

- **JT1078 RTP-Protokoll**: vollständige Analyse und Erzeugung von RTP-Nachrichten mit 29 Audio-/Videotypen
- **Frame-Abgrenzung**: Sticky-Paket-Trennung und Header-First-Analyse auf Basis von NewLife.Core `IMessageCodec`
- **RTP-Fragmentzusammenführung**: automatische Pufferung und Zusammenführung von I/P/B-Fragmenten
- **Audiodecodierung**: G.711A, G.711U, IMA-ADPCM mit Entfernung des HiSilicon-Headers
- **H.264-Analyse**: NAL-Trennung, SPS/PPS, Startcodes und Emulation-Prevention-Bytes
- **Remuxing**: Echtzeit-FLV und fMP4, Ausgabe als HTTP-FLV/WS-FLV, fMP4 und HLS
- **Server**: TCP/UDP, HTTP/WS-Endpunkte, Wiedergabe und Cluster-Befehlbus (IEventBus)
- **NativeAOT**: Protokoll und Sprachtabellen ohne Reflexion; natives Image per Smoke-Test verifiziert
- **Lokalisierung**: englische Quelltexte, 10 Übersetzungen, englischer Fallback

## Schnellstart

```bash
dotnet add package NewLife.JT1078           # Protokollbibliothek (inkl. FLV/fMP4)
dotnet add package NewLife.JT1078.Server    # TCP/UDP-Server + HLS/MP3
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

// Oder ASP.NET Core-Integration
builder.Services.AddJT1078Gateway().AddTcp().AddHttp();
```

## NuGet-Pakete

| Paket | Beschreibung |
|---|---|
| NewLife.JT1078 | Protokoll + Codecs + FLV/fMP4-Remuxing |
| NewLife.JT1078.Server | TCP/UDP-Server + DI-Builder + HLS/MP3 |

## Native AOT

Die Protokollbibliothek und der Server mit Standardkonfiguration unterstützen NativeAOT und Trimming. Für net8.0 und höher sind die AOT-/Trim-Analysatoren dauerhaft aktiv, ohne IL-Warnungen. Das Projekt `AotSmoke` veröffentlicht in CI ein natives Image mit `PublishAot=true` und prüft Frame-Codecs sowie Sprachtabellen.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

## Lokalisierung

Ausnahmen und Protokolle nutzen eine zur Kompilierungszeit erzeugte Tabelle. Standard ist `CultureInfo.CurrentUICulture`; fehlende Übersetzungen fallen auf Englisch zurück. Kein resx, keine Satellitenassemblys: eine DLL, AOT-sicher.

```csharp
Localization.Culture = new CultureInfo("de");
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Enthaltene Sprachen: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. Neue Sprachen werden in `Locales/<culture>.yml` ergänzt.

## Build und Tests

```bash
dotnet test -c Release
```

Die Tests decken Frame-Codec, mehrsegmentige Sequenzen, ungültige Frames, Fragmentzusammenführung, BCD, Audiocodecs, H.264, FLV/fMP4, den Protokollmodus des Servers und Sprachtabellen ab.

## Ökosystem

Die vollständige Projektmatrix finden Sie in [README.md](README.md#ecosystem) (in allen NewLife-Repositories identisch).

## Lizenz

[MIT](LICENSE) © 2022-2026 NewLife
