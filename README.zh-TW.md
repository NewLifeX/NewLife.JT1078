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
🌐 [English](README.md) | [简体中文](README.zh-CN.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

JT/T 1078 道路運輸車輛衛星定位系統視訊通訊協定的 .NET 實作，涵蓋協定解析、音視訊編解碼、FLV/fMP4 轉封裝、TCP/UDP 伺服器、HTTP/WebSocket 直播推送全鏈路。支援 NativeAOT 與執行階段多語言。

## 功能特性

### 協定層
- **JT1078 RTP 協定**：完整的 RTP 封包解析與建構，支援 29 種音視訊編碼類型
- **訊框定界**：基於 NewLife.Core `IMessageCodec` 的黏包/拆包與標頭先行解析
- **RTP 分包合併**：自動快取並合併 I/P/B 訊框的分包

### 音視訊編解碼
- **音訊解碼**：G.711A (A-law)、G.711U (μ-law)、IMA-ADPCM，支援海思晶片標頭剝離
- **AAC 編碼**：基於 FaacEncoder (P/Invoke) 的 AAC 編碼
- **H.264 解析**：NAL 單元分割、SPS/PPS 解析、起始碼處理、競爭位元組剝離

### 容器轉封裝
- **FLV 轉封裝**：將 RTP 流即時轉換為 HTTP-FLV / WebSocket-FLV，支援 flv.js 播放
- **fMP4 轉封裝**：CMAF/fMP4 分段輸出，支援 ISO BMFF 核心 Box 類型

### 服務端
- **TCP 服務**：開箱即用的 JT1078 TCP 伺服器，支援連接埠設定、除錯日誌
- **UDP 服務**：支援 JT/T 1078 UDP 傳輸模式，與 TCP 雙棧共存
- **HTTP/WS 直播**：內建 ASP.NET Core 端點，支援 FLV/fMP4/AAC/PCM/WAV 格式
- **錄影回放**：支援依 SIM、通道與時間範圍的歷史音視訊回放
- **裝置命令**：基於 IEventBus 的叢集命令下發匯流排，支援單機/Redis 叢集模式

### 管理與整合
- **裝置線上狀態**：即時追蹤終端連線/斷開事件
- **串流授權**：Token 認證保護直播端點
- **DI 建構器**：`AddJT1078Gateway().AddTcp().AddHttp()` 鏈式註冊
- **IHostedService**：ASP.NET Core 生命週期自動管理
- **NativeAOT**：核心協定與多語言詞表零反射，原生映像建置通過冒煙驗證
- **多語言**：例外與日誌以英文為源文，內建 10 種語言譯文，缺翻譯自動回退英文

## 快速開始

### 安裝 NuGet 套件

```bash
dotnet add package NewLife.JT1078           # 核心協定庫（含 FLV/fMP4 轉封裝）
dotnet add package NewLife.JT1078.Server    # TCP/UDP 伺服器 + HLS/MP3
```

### 解析 RTP 封包

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// 訊框定界：標頭到齊即可產出訊息，負載由訊框層綁定
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // 檢視編碼類型
    if (msg.Type == AVTypes.H264)
    {
        var frame = new H264Decoder().ParseAVFrame(msg);
    }
}
```

### 音訊解碼

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// 裝置負載 → PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM → G.711A
var g711a = factory.FromPcm(NewLife.Audio.AVTypes.G711A, pcm);
```

### 啟動串流伺服器

```csharp
using NewLife.JT1078.Server;

// 方式一：直接使用
var server = new JT1078Server();
server.Start();

// 方式二：ASP.NET Core 整合
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## NuGet 套件

| 套件 | 說明 | NuGet |
|---------|-------------|-------|
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | 核心協定 + 編解碼 + FLV/fMP4 轉封裝（Flv/FMp4 已合併入核心） | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | TCP/UDP 伺服器 + DI 建構器 + HLS/MP3 編碼 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

核心協定庫與預設設定的服務端支援 NativeAOT 與裁剪。net8.0 以上目標常駐啟用 AOT/裁剪分析器，建置期零 IL 警告；`AotSmoke` 冒煙工程於 CI 以 `PublishAot=true` 發布原生映像並執行協定編解碼與多語言查表。

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> 外掛、動態腳本與執行階段反射註冊不在 AOT 支援範圍內（與 NewLife.Core 邊界一致）。

## 多語言本地化

例外與日誌共用一套編譯期產生的多語言詞表，預設跟隨 `CultureInfo.CurrentUICulture`，缺翻譯自動回退英文；不依賴 resx 與衛星組件，單一 DLL、原生支援 AOT。

| 設定 | 作用 |
|---|---|
| `CultureInfo.CurrentUICulture` | 預設語言來源，ASP.NET Core 可依請求變化 |
| `Localization.Culture` | 強制指定例外與一般文字語言 |
| `Localization.LogCulture` | 開發者可見文字（日誌、設定註解）的語言 |

```csharp
// 全站繁體中文
Localization.Culture = new CultureInfo("zh-TW");

// Web 服務依 Accept-Language 識別各請求語言
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

內建 `zh-Hans`、`zh-Hant`、`es`、`de`、`ja`、`fr`、`pt`、`ru`、`ko`、`ar` 十種譯文；新增語言只需在 `Locales/<culture>.yml` 補充英文源文到譯文的對應，來源產生器在建置期完成校驗、壓縮與註冊。

## 建置與測試

```bash
dotnet test -c Release
```

測試涵蓋訊框編解碼、多段序列、損壞訊框、訊息分包合併、BCD 序列化、音訊編解碼、H.264 解析、FLV/fMP4 封裝、服務端協定模式與多語言詞表。

## 架構

```
JT1078 終端裝置 (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  RTP 分包合併
    ▼
JTMessage (RTP 封包)
    ├──► H264Decoder (視訊)
    ├──► AudioCodecFactory (音訊)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV
    ├──► FMp4Encoder ────────► fMP4
    ├──► HlsHandler / TSMuxer ──► HLS (/hls.m3u8)
    ├──► Mp3Handler ─────────► MP3 音訊
    ├──► 音訊 Handler ────────► AAC / PCM / WAV
    │
    ├──► DeviceSessionManager ──► 叢集命令廣播 (IEventBus)
    │
    ▼
播放器 (flv.js / hls.js)
```

## 叢集部署

服務端支援多實例部署，透過事件匯流排將裝置命令轉發到其他實例：

```csharp
// 單機模式（預設，記憶體 EventBus）
services.AddJT1078Gateway().AddTcp().AddHttp();

// 叢集模式（透過 Redis/Stardust 提供 ICacheProvider）
// 自動建立 Redis EventBus，命令廣播到所有實例
```

REST API：
- `POST /Command/Send`：同步向裝置發送命令，自動路由到裝置所在實例
- `POST /Command/SendAsync`：非同步向裝置發送命令，支援 CancellationToken
- `GET /Command/Devices`：列出本地已連線的裝置（TCP/UDP）

## 直播端點

| 端點 | 格式 | 傳輸 | 播放器 |
|----------|--------|-----------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | 原生 MSE |
| `/live.aac` | AAC | HTTP | 原生音訊 |
| `/live.pcm` | PCM | HTTP | 原生音訊 |
| `/live.wav` | WAV | HTTP | 原生音訊 |
| `/live.mp3` | MP3 | HTTP | 原生音訊 |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## 協定參考

- JT/T 1078-2016《道路運輸車輛衛星定位系統 視訊通訊協定》
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## 致謝

本專案在設計與實作過程中參考了 [SmallChi/JT1078](https://github.com/SmallChi/JT1078) 開源專案，特此致謝。

## 生態版圖

NewLife 生態系按相依分層，各專案獨立版本、獨立發行。下面的版圖在所有 NewLife 儲存庫中逐字一致；每個專案連結到程式碼儲存庫與主 NuGet 套件。

| 專案 | 一句話定位 | 下載量 |
| --- | --- | --- |
| **基礎底座** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | 日誌、設定、快取、網路、序列化、排程與 APM 追蹤。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 資料中介層：單表百億級、自動分表、讀寫分離，支援 10 多種資料庫。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 高效能網路庫：單機 177 萬 pps、14.5 Gbps（TCP 1KB，2026-10 實測），400 萬 Tcp 長連線。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 協定通訊與 RPC 框架，面向 CS 應用程式與物聯網裝置。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | 將 .NET 應用程式安裝為作業系統常駐服務：Windows 服務與 Linux systemd。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new` 專案範本：主控台、Web、API、服務、資料與物聯網。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **資料儲存與訊息** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis 用戶端：微秒級延遲、百萬級吞吐，內建訊息佇列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 純受控 RocketMQ 用戶端，支援 Apache RocketMQ 與阿里雲訊息佇列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT 用戶端與服務端，含阿里雲物聯網接入。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 混合式資料庫：內嵌或網路雙模，關係型、時序、訊息佇列與 KV 於一體。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | 建構於 NewLife 資料堆疊之上的 MySql 驅動與輔助層。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web、AI 與分散式** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 快速後台開發框架：權限、SSO、OAuth 服務端與 CRUD 樣板。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 分散式服務平台：節點管理、APM、設定中心、註冊中心與發佈中心。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | 分散式任務與即時資料排程平台，支援分片、重試與回溯補算。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 統一接入 46 家大模型服務商，附帶開箱即用的 ChatAI Web 對話應用。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **工業與物聯網** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | 物聯網標準庫，定義通訊協定契約與裝置抽象。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | 建構於 NewLife.IoT 之上的 Modbus TCP/RTU/ASCII 實作。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | 建構於 NewLife.IoT 之上的西門子 PLC 協定。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | 建構於 NewLife.IoT 之上的歐姆龍 PLC 協定。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | 建構於 NewLife.IoT 之上的三菱 Melsec PLC 協定。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | 工業資料存取的 OPC 用戶端。 | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet 樓宇自動化協定堆疊。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | 開源輕量級物聯網平台，含閘道與裝置管理。 | — |
| **車聯網** | | |
| [NewLife.JT808](https://github.com/NewLifeX/NewLife.JT808) | JT/T 808 車輛定位協定：70+ 訊息體，涵蓋 2011/2013/2019 三個版本。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT808) |
| [NewLife.JT809](https://github.com/NewLifeX/NewLife.JT809) | JT/T 809 部標平台互聯協定，面向運輸管理部門。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| [NewLife.JT1078](https://github.com/NewLifeX/NewLife.JT1078) | JT/T 1078 車載視訊：RTP 解析、FLV/fMP4 轉封裝與直播推送。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT1078) |
| **領域元件** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | 一個套件涵蓋 Excel、Word、PPT、PDF 等 15+ 文件格式。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 跨平台音訊：編解碼、裝置 IO、DSP、容器與串流處理。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | 視訊編解碼、封裝格式、影格處理與串流。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 百度／高德／騰訊／天地圖統一介面，支援地理編碼與座標轉換。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 歸屬地：離線記憶體映射資料庫 + 線上 API 雙來源。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **工具與產品** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 開發者工具箱：網路、序列埠、加解密、正規表示式、Modbus 與 MQTT。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 自架 Git 原始碼管理系統。 | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 從較舊的宿主啟動時，自動安裝相符的 .NET 執行階段。 | — |

> 徽章取各專案主套件；主題套件、擴充套件與衛星套件見對應 NuGet 頁面。未在表中列出的 30 多個套件（FTP、LoRa、Office 擴充、各類協定驅動等）見 [NewLifeX 組織](https://github.com/NewLifeX) 與 [newlifex.com](https://newlifex.com)。

## 新生命開發團隊

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命團隊成立於 2002 年，二十餘年專注 .NET 與物聯網技術生態，主導開源專案近百個（生態版圖收錄 34 個主幹專案），為電力、物流、醫療、工控、網際網路等行業提供軟硬體一體化解決方案；以「學無先後達者為師」為信條。

開源元件在 NuGet 累計下載 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 次（全生態口徑，截至 2026-10）。

網站：https://newlifex.com · 開源組織：https://github.com/NewLifeX

<details>
<summary>微信公眾號：智能大石头（掃碼關注）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## 授權

[MIT](LICENSE) © 2022-2026 新生命開發團隊