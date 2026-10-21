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

### Protokoll
- **JT1078 RTP-Protokoll**: vollständige Analyse und Erzeugung von RTP-Nachrichten mit 29 Audio-/Videotypen
- **Frame-Abgrenzung**: Sticky-Paket-Trennung und Header-First-Analyse auf Basis von NewLife.Core `IMessageCodec`
- **RTP-Fragmentzusammenführung**: automatische Pufferung und Zusammenführung von I/P/B-Frame-Fragmenten

### Audio- und Videocodecs
- **Audiodecodierung**: G.711A (A-law), G.711U (μ-law), IMA-ADPCM, mit Entfernung des HiSilicon-Headers
- **AAC-Kodierung**: AAC-Kodierung über FaacEncoder (P/Invoke)
- **H.264-Analyse**: NAL-Unit-Trennung, SPS/PPS-Analyse, Startcode-Behandlung, Entfernen von Emulation-Prevention-Bytes

### Container-Remuxing
- **FLV-Remuxing**: RTP in Echtzeit nach HTTP-FLV / WebSocket-FLV für die Wiedergabe mit flv.js
- **fMP4-Remuxing**: CMAF/fMP4-Segmentausgabe mit den zentralen ISO-BMFF-Boxes

### Server
- **TCP-Dienst**: sofort einsatzbereiter JT1078-TCP-Server mit Portkonfiguration und Debug-Protokollierung
- **UDP-Dienst**: JT/T 1078-UDP-Transport parallel zu TCP
- **HTTP/WS-Livestreaming**: integrierte ASP.NET Core-Endpunkte für FLV/fMP4/AAC/PCM/WAV
- **Wiedergabe**: Wiedergabe historischer Audio-/Videodaten nach SIM, Kanal und Zeitbereich
- **Gerätebefehle**: Cluster-Befehlbus auf Basis von IEventBus, eigenständig oder als Redis-Cluster

### Verwaltung und Integration
- **Gerätepräsenz**: Echtzeitverfolgung von Verbindungsaufbau und -abbau der Terminals
- **Stream-Autorisierung**: Token-Authentifizierung zum Schutz der Live-Endpunkte
- **DI-Builder**: flüssige Registrierung mit `AddJT1078Gateway().AddTcp().AddHttp()`
- **IHostedService**: automatische Lebenszyklusverwaltung in ASP.NET Core
- **NativeAOT**: reflexionsfreie Protokoll- und Sprachtabellen, natives Image durch Smoke-Test verifiziert
- **Lokalisierung**: englische Quelltexte mit 10 integrierten Übersetzungen und englischem Fallback

## Schnellstart

### NuGet-Pakete installieren

```bash
dotnet add package NewLife.JT1078           # Kernprotokollbibliothek (inkl. FLV/fMP4-Remuxing)
dotnet add package NewLife.JT1078.Server    # TCP/UDP-Server + HLS/MP3
```

### Eine RTP-Nachricht parsen

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// Frame-Abgrenzung: Sobald der Header eintrifft, entsteht eine Nachricht; der Body wird von der Frame-Schicht gebunden
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // Payload-Typ prüfen
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### Audiodecodierung

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// Geräte-Payload -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### Streaming-Server starten

```csharp
using NewLife.JT1078.Server;

// Option 1: direkt verwenden
var server = new JT1078Server();
server.Start();

// Option 2: ASP.NET Core-Integration
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## NuGet-Pakete

| Paket | Beschreibung | NuGet |
|---------|-------------|-------|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Kernprotokoll + Codecs + FLV/fMP4-Remuxing (Flv/FMp4 in den Kern integriert) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | TCP/UDP-Server + DI-Builder + HLS/MP3-Kodierung | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

Die Kernprotokollbibliothek und der Server in Standardkonfiguration unterstützen NativeAOT und Trimming. Für net8.0 und höher sind die AOT-/Trim-Analysatoren dauerhaft aktiv und erzeugen zur Buildzeit keine IL-Warnungen. Das Projekt `AotSmoke` in diesem Repository veröffentlicht in CI ein natives Image mit `PublishAot=true` und prüft Frame-Codecs, Reflexionsserialisierung der Modelle, DI-Zusammenbau, Server-Zusammenbau und Sprachtabellen-Abfragen.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> Plugins, dynamisches Skripting und Registrierung per Laufzeitreflexion liegen außerhalb der AOT-Unterstützungsgrenze (konsistent mit NewLife.Core).

## Lokalisierung

Ausnahmen und Protokolle nutzen eine zur Kompilierungszeit erzeugte Tabelle. Standard ist `CultureInfo.CurrentUICulture`; fehlende Übersetzungen fallen auf Englisch zurück. Kein resx, keine Satellitenassemblys: eine DLL, AOT-sicher.

| Einstellung | Zweck |
|---|---|
| `CultureInfo.CurrentUICulture` | Standardquelle für die Sprache; ASP.NET Core kann sie pro Anforderung variieren |
| `Localization.Culture` | Erzwingt die Sprache für Ausnahmen und normalen Text |
| `Localization.LogCulture` | Sprache der entwicklernahen Texte (Protokolle, Konfigurationskommentare) |

```csharp
// Websiteweit Chinesisch
Localization.Culture = new CultureInfo("zh-CN");

// Webdienst, der Accept-Language pro Anforderung berücksichtigt
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Integrierte Übersetzungen (10): `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. Neue Sprachen werden in `Locales/<culture>.yml` ergänzt; der Source Generator validiert, komprimiert und registriert die Tabelle zur Buildzeit.

## Build und Tests

```bash
dotnet test -c Release
```

Die Tests decken Frame-Codec, mehrsegmentige Sequenzen, ungültige Frames, Fragmentzusammenführung, BCD, Audiocodecs, H.264, FLV/fMP4, den Protokollmodus des Servers und Sprachtabellen ab.

## Architektur

```
JT1078-Gerät (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  RTP-Fragmentzusammenführung
    ▼
JTMessage (RTP-Nachricht)
    ├──► H264Decoder (Video)
    ├──► AudioCodecFactory (Audio)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► MP3-Audio
    ├──► Audio-Handler ──────► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► Cluster-Befehls-Broadcast (IEventBus)
    │
    ▼
Player (flv.js / hls.js)
```

## Cluster-Bereitstellung

Der Server unterstützt die Bereitstellung mit mehreren Instanzen und leitet Gerätebefehle über einen Event-Bus an die jeweilige Instanz weiter:

```csharp
// Eigenständig (Standard, EventBus im Arbeitsspeicher)
services.AddJT1078Gateway().AddTcp().AddHttp();

// Cluster (ICacheProvider über Redis/Stardust bereitstellen)
// Ein Redis-EventBus wird automatisch erstellt und Befehle an alle Instanzen verteilt
```

REST-API:
- `POST /Command/Send`: Befehl synchron an ein Gerät senden, automatisch an die Instanz weitergeleitet, die das Gerät hält
- `POST /Command/SendAsync`: Befehl asynchron senden, mit Unterstützung für CancellationToken
- `GET /Command/Devices`: lokal verbundene Geräte auflisten (TCP/UDP)

## Live-Endpunkte

| Endpunkt | Format | Transport | Player |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | native MSE |
| `/live.aac` | AAC | HTTP | native Audio |
| `/live.pcm` | PCM | HTTP | native Audio |
| `/live.wav` | WAV | HTTP | native Audio |
| `/live.mp3` | MP3 | HTTP | native Audio |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## Protokollreferenzen

- JT/T 1078-2016, Satellitenpositionierungssystem für Straßentransportfahrzeuge – Videokommunikationsprotokoll
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## Danksagung

Design und Umsetzung dieses Projekts orientieren sich am Open-Source-Projekt [SmallChi/JT1078](https://github.com/SmallChi/JT1078). Vielen Dank.

## Ökosystem-Karte

NewLife ist ein geschichtetes Ökosystem unabhängig versionierter Open-Source-Projekte. Die folgende Karte wird wortgleich in allen NewLife-Repositories verwendet; jedes Projekt verlinkt auf sein Repository und sein primäres NuGet-Paket.

| Projekt | Funktion | Downloads |
| --- | --- | --- |
| **Grundlage** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Protokollierung, Konfiguration, Caching, Netzwerk, Serialisierung, Zeitplanung und APM-Tracing. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Daten-Middleware: einzelne Tabellen in der Größenordnung von zig Milliarden Zeilen, automatisches Sharding, Lese-/Schreibtrennung, über 10 Datenbanken. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | Hochperformante Netzwerkbibliothek: 1,77 Mio. pps / 14,5 Gbps auf einer Maschine (TCP 1KB, 2026-10), 4 Mio. langlebige TCP-Verbindungen. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Protokollkommunikation und RPC-Framework für Client/Server-Anwendungen und IoT-Geräte. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installiert .NET-Anwendungen als Betriebssystem-Dienste: Windows-Dienste und Linux-systemd. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new`-Projektvorlagen für Konsole, Web, API, Dienst, Daten und IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Daten & Messaging** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis-Client mit Mikrosekunden-Latenz, Millionen Operationen pro Sekunde und integrierten Nachrichtenwarteschlangen. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Vollständig verwalteter RocketMQ-Client für Apache RocketMQ und Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT-Client und -Server, einschließlich Alibaba Cloud IoT-Anbindung. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Hybriddatenbank in C#: eingebettet oder im Netzwerk, relational, Zeitreihen, Nachrichtenwarteschlange und KV in einem. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | MySql-Treiber und Hilfsschicht auf Basis des NewLife-Datenstacks. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, KI & verteilt** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Schnelles Framework für Administrationsplattformen: Berechtigungen, SSO, OAuth-Server und CRUD-Gerüst. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Verteilte Dienstplattform: Knotenverwaltung, APM, Konfigurationszentrum, Registry und Release-Center. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Plattform für verteilte Aufgaben und Echtzeit-Datenplanung mit Sharding, Wiederholungen und Backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Einheitlicher Client für 46 LLM-Anbieter plus ChatAI, eine sofort einsatzbereite Web-Chat-Anwendung. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrie & IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | IoT-Standardbibliothek mit Protokollverträgen und Geräteabstraktionen. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Modbus-TCP/RTU/ASCII-Implementierung auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Siemens-SPS-Protokoll auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Omron-SPS-Protokoll auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Mitsubishi-Melsec-SPS-Protokoll auf Basis von NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | OPC-Client für industriellen Datenzugriff. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet-Protokollstack für die Gebäudeautomation. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Leichtgewichtige Open-Source-IoT-Plattform mit Gateway- und Geräteverwaltung. | — |
| **Fahrzeugtelematik** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T-808-Fahrzeugortungsprotokoll: über 70 Nachrichtenkörper, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T-809-Plattform-zu-Plattform-Protokoll für Verkehrsbehörden. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T-1078-Fahrzeugvideo: RTP-Parsing, FLV/fMP4-Remuxing und Live-Streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Domänenkomponenten** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | Ein Paket für Excel, Word, PPT, PDF und über 15 Dokumentformate. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Plattformübergreifendes Audio: Codecs, Geräte-I/O, DSP, Container und Streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Videocodecs, Containerformate, Frame-Verarbeitung und Streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Einheitliche Schnittstelle für Baidu, AMap, Tencent und Tianditu mit Geokodierung und Koordinatenumrechnung. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4-Geolokalisierung mit einer offline speichergemappten Datenbank und einer Online-API als Fallback. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Werkzeuge & Produkte** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Entwickler-Toolbox: Netzwerk, serielle Schnittstelle, Krypto, Regex, Modbus und MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Selbst gehostetes Git-Quellcode-Verwaltungssystem. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installiert automatisch die passende .NET-Laufzeit, wenn es von einem älteren Host gestartet wird. | — |

> Die Badges zeigen das primäre Paket jedes Projekts; Theme-, Erweiterungs- und Satellitenpakete sind auf den jeweiligen NuGet-Seiten aufgeführt. Die hier nicht aufgeführten über 30 Pakete (FTP, LoRa, Office-Erweiterungen, Protokolltreiber und mehr) sind in der [NewLifeX-Organisation](https://github.com/NewLifeX) und auf [newlifex.com](https://newlifex.com) verfügbar.

## Team

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

NewLife wurde 2002 gegründet und widmet sich seit mehr als zwei Jahrzehnten dem .NET- und IoT-Ökosystem, betreut fast 100 Open-Source-Projekte (34 davon Hauptprojekte) und liefert integrierte Hard- und Softwarelösungen für die Bereiche Energie, Logistik, Gesundheitswesen, Industrieautomation und Internet. Unser Leitspruch lautet „Lernen kennt kein Dienstalter – wer es beherrscht, wird zum Lehrmeister“.

Die Open-Source-Komponenten wurden <!--nuget-total:start-->6.48M<!--nuget-total:end--> Mal von NuGet heruntergeladen (ökosystemweit, Stand 2026-10).

Website: https://newlifex.com · Open-Source-Organisation: https://github.com/NewLifeX

<details>
<summary>WeChat-Offizieller Account: 智能大石头 (zum Folgen scannen)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## Lizenz

[MIT](LICENSE) © 2022-2026 NewLife
