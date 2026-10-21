<!-- generated from README.md by sync-readme-package.ps1; do not edit -->

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
🌐 [简体中文](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.zh-CN.md) | [繁體中文](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.zh-TW.md) | [Español](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.es.md) | [Deutsch](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.de.md) | [日本語](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.ja.md) | [Français](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.fr.md) | [Português](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.pt.md) | [Русский](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.ru.md) | [한국어](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.ko.md) | [العربية](https://github.com/NewLifeX/NewLife.JT1078/blob/master/README.ar.md)
<!-- lang:end -->

A .NET implementation of the JT/T 1078 video communication protocol for road transport satellite positioning systems. It covers the full pipeline of protocol parsing, audio/video codecs, FLV/fMP4 remuxing, TCP/UDP servers, and HTTP/WebSocket live streaming. NativeAOT and runtime localization are supported.

## Features

### Protocol
- **JT1078 RTP protocol**: complete RTP message parsing and building with 29 audio/video payload types
- **Frame framing**: sticky-packet splitting and header-first parsing built on NewLife.Core `IMessageCodec`
- **RTP fragment merging**: automatic buffering and merging of I/P/B frame fragments

### Audio and video codecs
- **Audio decoding**: G.711A (A-law), G.711U (μ-law), IMA-ADPCM, with HiSilicon header stripping
- **AAC encoding**: AAC encoding via FaacEncoder (P/Invoke)
- **H.264 parsing**: NAL unit splitting, SPS/PPS parsing, start-code handling, emulation-prevention byte removal

### Container remuxing
- **FLV remuxing**: real-time RTP to HTTP-FLV / WebSocket-FLV for flv.js playback
- **fMP4 remuxing**: CMAF/fMP4 segment output with core ISO BMFF boxes

### Server
- **TCP service**: out-of-the-box JT1078 TCP server with port configuration and debug logging
- **UDP service**: JT/T 1078 UDP transport coexisting with TCP
- **HTTP/WS live streaming**: built-in ASP.NET Core endpoints for FLV/fMP4/AAC/PCM/WAV
- **Playback**: historical audio/video playback by SIM, channel and time range
- **Device commands**: cluster command bus based on IEventBus, standalone or Redis cluster

### Management and integration
- **Device presence**: real-time tracking of terminal connect/disconnect events
- **Stream authorization**: token authentication protecting live endpoints
- **DI builder**: `AddJT1078Gateway().AddTcp().AddHttp()` fluent registration
- **IHostedService**: automatic ASP.NET Core lifecycle management
- **NativeAOT**: reflection-free protocol and localization tables, native image verified by smoke test
- **Localization**: English source messages with 10 bundled translations and English fallback

## Quick Start

### Install NuGet packages

```bash
dotnet add package NewLife.JT1078           # core protocol library (includes FLV/fMP4 remuxing)
dotnet add package NewLife.JT1078.Server    # TCP/UDP server + HLS/MP3
```

### Parse an RTP message

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// Frame framing: a message is produced as soon as the header arrives; the body is bound by the frame layer
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // Inspect the payload type
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### Audio decoding

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// Device payload -> PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM -> G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### Start the streaming server

```csharp
using NewLife.JT1078.Server;

// Option 1: use directly
var server = new JT1078Server();
server.Start();

// Option 2: ASP.NET Core integration
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## NuGet Packages

| Package | Description | NuGet |
|---------|-------------|-------|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | Core protocol + codecs + FLV/fMP4 remuxing (Flv/FMp4 merged into the core) | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | TCP/UDP server + DI builder + HLS/MP3 encoding | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

The core protocol library and the server under default configuration support NativeAOT and trimming. The AOT/trim analyzers are always enabled for net8.0 and later targets with zero IL warnings at build time. The `AotSmoke` project in this repository publishes a native image in CI with `PublishAot=true` and exercises frame codecs, model reflection serialization, DI assembly, server assembly and localization lookups.

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> Plugins, dynamic scripting and runtime reflection registration are outside the AOT support boundary (consistent with NewLife.Core).

## Localization

Exceptions and logs share a compile-time generated localization table. It follows `CultureInfo.CurrentUICulture` by default and falls back to English when a translation is missing. No resx or satellite assemblies are involved: a single DLL, natively AOT-safe.

| Setting | Purpose |
|---|---|
| `CultureInfo.CurrentUICulture` | Default language source; ASP.NET Core can vary it per request |
| `Localization.Culture` | Forces the language for exceptions and regular text |
| `Localization.LogCulture` | Language of developer-facing text (logs, configuration comments) |

```csharp
// Site-wide Chinese
Localization.Culture = new CultureInfo("zh-CN");

// Web service honoring Accept-Language per request
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

Bundled translations: `zh-Hans`, `zh-Hant`, `es`, `de`, `ja`, `fr`, `pt`, `ru`, `ko`, `ar`. To add a language, map English source strings to translations in `Locales/<culture>.yml`; the source generator validates, compresses and registers the table at build time.

## Build and Test

```bash
dotnet test -c Release
```

Tests cover frame codec framing/building, multi-segment sequences, invalid frames, message fragment merging, BCD serialization, audio codecs, H.264 parsing, FLV/fMP4 packaging, server protocol mode and localization tables.

## Architecture

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

## Cluster Deployment

The server supports multi-instance deployment and forwards device commands across instances through an event bus:

```csharp
// Standalone (default, in-memory EventBus)
services.AddJT1078Gateway().AddTcp().AddHttp();

// Cluster (provide ICacheProvider via Redis/Stardust)
// A Redis EventBus is created automatically and commands are broadcast to all instances
```

REST API:
- `POST /Command/Send`: send a command to a device synchronously, automatically routed to the instance holding the device
- `POST /Command/SendAsync`: send a command asynchronously with CancellationToken support
- `GET /Command/Devices`: list locally connected devices (TCP/UDP)

## Live Endpoints

| Endpoint | Format | Transport | Player |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | native MSE |
| `/live.aac` | AAC | HTTP | native audio |
| `/live.pcm` | PCM | HTTP | native audio |
| `/live.wav` | WAV | HTTP | native audio |
| `/live.mp3` | MP3 | HTTP | native audio |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## Protocol References

- JT/T 1078-2016, Road transport vehicles satellite positioning system - Video communication protocol
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## Acknowledgements

The design and implementation of this project refer to the open-source project [SmallChi/JT1078](https://github.com/SmallChi/JT1078). Many thanks.

## Ecosystem Map

NewLife is a layered ecosystem of independently versioned open-source projects. The map below is shared verbatim by every NewLife repository; each project links to its repository and its primary NuGet package.

| Project | What it does | Downloads |
| --- | --- | --- |
| **Foundation** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | Logging, configuration, caching, networking, serialization, scheduling and APM tracing. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | Data middleware: single tables at the tens-of-billions scale, automatic sharding, read-write splitting, 10+ databases. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | High-performance network library: 1.77M pps / 14.5 Gbps on a single machine (TCP 1KB, 2026-10), 4M long-lived TCP connections. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | Protocol communication and RPC framework for client/server applications and IoT devices. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | Installs .NET applications as OS daemons: Windows services and Linux systemd. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new` project templates for console, web, API, service, data and IoT projects. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **Data & Messaging** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis client with microsecond latency, millions of ops/s and built-in message queues. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | Pure managed RocketMQ client for Apache RocketMQ and Alibaba Cloud Message Queue. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT client and server, including Alibaba Cloud IoT connectivity. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | Hybrid database in C#: embedded or networked, relational, time-series, message queue and KV in one. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | MySql driver and helper layer built on the NewLife data stack. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web, AI & Distributed** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | Rapid admin-platform framework: permissions, SSO, OAuth server and CRUD scaffolding. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | Distributed service platform: node management, APM, configuration center, registry and release center. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | Distributed task and real-time data scheduling platform with sharding, retries and backfill. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | Unified client for 46 LLM providers plus ChatAI, a ready-to-run web chat application. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **Industrial & IoT** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | IoT standard library defining protocol contracts and device abstractions. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Modbus TCP/RTU/ASCII implementation on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | Siemens PLC protocol on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | Omron PLC protocol on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | Mitsubishi Melsec PLC protocol on top of NewLife.IoT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | OPC client for industrial data access. | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet building-automation protocol stack. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | Lightweight open-source IoT platform with gateway and device management. | — |
| **Vehicle Telematics** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 vehicle positioning protocol: 70+ message bodies, 2011/2013/2019. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T 809 platform-to-platform protocol for transport authorities. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 in-vehicle video: RTP parsing, FLV/fMP4 remuxing and live streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **Domain Components** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | One package for Excel, Word, PPT, PDF and 15+ document formats. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | Cross-platform audio: codecs, device I/O, DSP, containers and streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | Video codecs, container formats, frame processing and streaming. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | Unified interface over Baidu, AMap, Tencent and Tianditu with geocoding and coordinate conversion. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 geolocation with an offline memory-mapped database and an online API fallback. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **Tools & Products** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | Developer toolbox: network, serial port, crypto, regex, Modbus and MQTT. | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | Self-hosted Git source code management system. | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | Installs the matching .NET runtime automatically when launched from an older host. | — |

> Badges show the primary package of each project; theme, extension and satellite packages are listed on their NuGet pages. The 30+ packages not listed here (FTP, LoRa, Office extensions, protocol drivers and more) are available under the [NewLifeX organization](https://github.com/NewLifeX) and on [newlifex.com](https://newlifex.com).

## Team

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

Founded in 2002, the NewLife team has spent more than two decades on the .NET and IoT ecosystem, maintains nearly 100 open-source projects (34 of them core to this map), and delivers integrated software and hardware solutions for power, logistics, healthcare, industrial control and internet industries. We live by the motto "learning has no seniority — mastery earns respect".

Open-source components have been downloaded <!--nuget-total:start-->6.48M<!--nuget-total:end--> times from NuGet (ecosystem-wide, as of 2026-10).

Website: https://newlifex.com · Open source organization: https://github.com/NewLifeX

<details>
<summary>WeChat Official Account: 智能大石头 (scan to follow)</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## License

[MIT](https://github.com/NewLifeX/NewLife.JT1078/blob/master/LICENSE) © 2022-2026 NewLife
