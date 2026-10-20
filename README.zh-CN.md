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
🌐 [English](README.md) | [繁體中文](README.zh-TW.md) | [Español](README.es.md) | [Deutsch](README.de.md) | [日本語](README.ja.md) | [Français](README.fr.md) | [Português](README.pt.md) | [Русский](README.ru.md) | [한국어](README.ko.md) | [العربية](README.ar.md)
<!-- lang:end -->

JT/T 1078 道路运输车辆卫星定位系统视频通讯协议的 .NET 实现，覆盖协议解析、音视频编解码、FLV/fMP4 转封装、TCP/UDP 服务端、HTTP/WebSocket 直播推送全链路。支持 NativeAOT 与运行时多语言。

## 功能特性

### 协议层
- **JT1078 RTP 协议**：完整的 RTP 报文解析与构建，支持 29 种音视频编码类型
- **帧定界**：基于 NewLife.Core `IMessageCodec` 的粘包/拆包与头部先行解析
- **RTP 分包合并**：自动缓存并合并 I/P/B 帧的分包

### 音视频编解码
- **音频解码**：G.711A (A-law)、G.711U (μ-law)、IMA-ADPCM，支持海思芯片头剥离
- **AAC 编码**：基于 FaacEncoder (P/Invoke) 的 AAC 编码
- **H.264 解析**：NAL 单元分割、SPS/PPS 解析、起始码处理、竞争字节剥离

### 容器封装
- **FLV 转封装**：将 RTP 流实时转换为 HTTP-FLV / WebSocket-FLV，支持 flv.js 播放
- **fMP4 转封装**：CMAF/fMP4 分段输出，支持 ISO BMFF 核心 Box 类型

### 服务端
- **TCP 服务**：开箱即用的 JT1078 TCP 服务器，支持端口配置、调试日志
- **UDP 服务**：支持 JT/T 1078 UDP 传输模式，与 TCP 双栈共存
- **HTTP/WS 直播**：内置 ASP.NET Core 端点，支持 FLV/fMP4/AAC/PCM/WAV 格式
- **录像回放**：支持按 SIM/通道/时间范围的历史音视频回放
- **设备命令**：基于 IEventBus 的集群命令下发总线，支持单机/Redis 集群模式

### 管理与集成
- **设备在线状态**：实时跟踪终端连接/断开事件
- **流媒体授权**：Token 认证保护直播端点
- **DI 构建器**：`AddJT1078Gateway().AddTcp().AddHttp()` 链式注册
- **IHostedService**：ASP.NET Core 生命周期自动管理
- **NativeAOT**：核心协议与多语言词表零反射，原生镜像构建通过冒烟验证
- **多语言**：异常与日志以英文为源文，内置 10 种语言译文，缺翻译自动回退英文

## 快速开始

### 安装 NuGet 包

```bash
dotnet add package NewLife.JT1078           # 核心协议库（含 FLV/fMP4 转封装）
dotnet add package NewLife.JT1078.Server    # TCP/UDP 服务端 + HLS/MP3
```

### 解析 RTP 报文

```csharp
using System.Buffers;
using NewLife.JT1078.Protocols;

// 帧定界：头部到齐即可产出消息，负载由帧层绑定
var codec = new JTCodec();
var result = codec.TryParse(new ReadOnlySequence<Byte>(data));
if (result is { Invalid: false } rs && rs.Message is JTMessage msg)
{
    // 检查编码类型
    if (msg.Type == AVTypes.H264)
    {
        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);
    }
}
```

### 音频解码

```csharp
using NewLife.Audio;

var factory = new AudioCodecFactory();
// ADPCM → PCM
var pcm = factory.ToPcm(NewLife.Audio.AVTypes.ADPCMA, packet);
// PCM → G.711A
var g711a = factory.FromPcm(pcmData, NewLife.Audio.AVTypes.G711A, null);
```

### 启动流媒体服务器

```csharp
using NewLife.JT1078.Server;

// 方式一：直接使用
var server = new JT1078Server();
server.Start();

// 方式二：ASP.NET Core 集成
builder.Services.AddJT1078Gateway()
       .AddTcp()
       .AddHttp();
```

## NuGet 包列表

| 包名 | 说明 | NuGet |
|------|------|-------|
| NewLife.JT1078 | 核心协议 + 编解码 + FLV/fMP4 转封装（Flv/FMp4 已合并入核心） | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Server | TCP/UDP 服务端 + DI 构建器 + HLS/MP3 编码 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## Native AOT

核心协议库与默认配置下的服务端支持 NativeAOT 与裁剪：net8.0 及以上目标常驻启用 AOT/裁剪分析器，构建期零 IL 告警；仓库内 `AotSmoke` 冒烟工程在 CI 中以 `PublishAot=true` 发布原生镜像并运行协议编解码、服务端装配与多语言查表。

```bash
dotnet publish AotSmoke/AotSmoke.csproj -c Release -r linux-x64 -p:PublishAot=true -o out
./out/AotSmoke
```

> 插件、动态脚本与运行时反射注册不属于 AOT 支持范围（与 NewLife.Core 边界一致）。

## 多语言本地化

异常与日志共享一套编译期生成的多语言词表，默认跟随 `CultureInfo.CurrentUICulture`，缺翻译自动回退英文；不依赖 resx 与卫星程序集，单 DLL、原生支持 AOT。

| 设置 | 作用 |
|---|---|
| `CultureInfo.CurrentUICulture` | 默认语言来源，ASP.NET Core 可按请求变化 |
| `Localization.Culture` | 强制指定异常与常规文本语言 |
| `Localization.LogCulture` | 开发者可见文本（日志、配置注释）的语言 |

```csharp
// 全站中文
Localization.Culture = new CultureInfo("zh-CN");

// Web 服务按 Accept-Language 识别请求语言
using var culture = RequestCultureScope.Create(Request.Headers.AcceptLanguage);
```

内置 `zh-Hans`、`zh-Hant`、`es`、`de`、`ja`、`fr`、`pt`、`ru`、`ko`、`ar` 十种译文；新增语言只需在 `Locales/<culture>.yml` 补充英文源文到译文的映射，源生成器在编译期完成校验、压缩与注册。

## 测试

```bash
dotnet test -c Release
```

测试覆盖协议帧编解码（定界/构建/多段序列/损坏帧）、消息分包合并、BCD 序列化、音频编解码、H264 解析、FLV/fMP4 封装、服务端协议模式与多语言词表。

## 架构

```
JT1078 终端设备 (TCP/UDP:1078)
    │
    ▼
NewLife.JT1078.Server ─── JT1078Session / JT1078UdpSession
    │  RTP 分包合并
    ▼
JTMessage (RTP 报文)
    ├──► H264Decoder (视频)
    ├──► AudioCodecFactory (音频)
    │
    ├──► FlvEncoder ──────────► HTTP-FLV / WS-FLV 直播
    ├──► FMp4Encoder ────────► fMP4 直播
    ├──► HlsHandler / TSMuxer ──► HLS 直播 (/hls.m3u8)
    ├──► Mp3Handler ─────────► MP3 音频直播
    ├──► 音频 Handler ────────► AAC / PCM / WAV 直播
    │
    ├──► DeviceSessionManager ──► 集群命令广播 (IEventBus)
    │
    ▼
 播放器 (flv.js / hls.js)
```

## 集群部署

服务端支持多实例集群部署，通过事件总线实现设备命令的跨实例转发：

```csharp
// 单机模式（默认，内存 EventBus）
services.AddJT1078Gateway().AddTcp().AddHttp();

// 集群模式（通过 Redis/Stardust 提供 ICacheProvider）
// 自动创建 Redis EventBus，命令广播到所有实例
```

REST API：
- `POST /Command/Send` — 向设备发送信令（同步，自动路由到设备所在服务器）
- `POST /Command/SendAsync` — 向设备发送信令（异步，CancellationToken 支持）
- `GET /Command/Devices` — 查询本地已连接的设备列表（含 TCP/UDP 类型）

## 直播端点

| 端点 | 格式 | 传输 | 播放器 |
|------|------|------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | 原生 MSE |
| `/live.aac` | AAC | HTTP | 原生音频 |
| `/live.pcm` | PCM | HTTP | 原生音频 |
| `/live.wav` | WAV | HTTP | 原生音频 |
| `/live.mp3` | MP3 | HTTP | 原生音频 |
| `/hls.m3u8` + `/hls.ts` | HLS (MPEG-TS) | HTTP | hls.js |

## 协议参考

- JT/T 1078-2016《道路运输车辆卫星定位系统 视频通讯协议》
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## 致谢

本项目在设计和实现过程中参考了 [SmallChi/JT1078](https://github.com/SmallChi/JT1078) 开源项目，特此致谢。

## 生态版图

NewLife 生态按依赖分层，各项目独立版本、独立发版。下面的版图在所有 NewLife 仓库中逐字一致；每个项目链接到代码仓库与主 NuGet 包。

| 项目 | 一句话定位 | 下载量 |
| --- | --- | --- |
| **基础底座** | | |
| [NewLife.Core](https://github.com/NewLifeX/X) | 日志、配置、缓存、网络、序列化、定时调度与 APM 追踪。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Core?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Core) |
| [NewLife.XCode](https://github.com/NewLifeX/NewLife.XCode) | 数据中间件：单表百亿级、自动分表、读写分离，支持 10 多种数据库。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCode?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCode) |
| [NewLife.Net](https://github.com/NewLifeX/NewLife.Net) | 高性能网络库：单机 177 万 pps、14.5 Gbps（TCP 1KB，2026-10 实测），400 万 Tcp 长连接。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Net?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Net) |
| [NewLife.Remoting](https://github.com/NewLifeX/NewLife.Remoting) | 协议通信与 RPC 框架，面向 CS 应用与物联网设备。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Remoting?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Remoting) |
| [NewLife.Agent](https://github.com/NewLifeX/NewLife.Agent) | 把 .NET 应用安装为操作系统守护进程：Windows 服务与 Linux systemd。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Agent?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Agent) |
| [NewLife.Templates](https://github.com/NewLifeX/NewLife.Zero) | `dotnet new` 项目模板：控制台、Web、WebApi、服务、数据与物联网。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Templates?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Templates) |
| **数据存储与消息** | | |
| [NewLife.Redis](https://github.com/NewLifeX/NewLife.Redis) | Redis 客户端：微秒级延迟、百万级吞吐，内置消息队列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Redis?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Redis) |
| [NewLife.RocketMQ](https://github.com/NewLifeX/NewLife.RocketMQ) | 纯托管 RocketMQ 客户端，支持 Apache RocketMQ 与阿里云消息队列。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.RocketMQ?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.RocketMQ) |
| [NewLife.MQTT](https://github.com/NewLifeX/NewLife.MQTT) | MQTT 客户端与服务端，客户端支持阿里云物联网。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MQTT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MQTT) |
| [NewLife.NovaDb](https://github.com/NewLifeX/NewLife.NovaDb) | C# 混合数据库：嵌入/网络双模，关系型、时序、消息队列与 KV 于一体。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.NovaDb?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.NovaDb) |
| [NewLife.MySql](https://github.com/NewLifeX/NewLife.MySql) | MySql 驱动与数据访问辅助层，基于 NewLife 数据栈实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.MySql?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.MySql) |
| **Web、AI 与分布式** | | |
| [NewLife.Cube](https://github.com/NewLifeX/NewLife.Cube) | 魔方快速开发平台：用户权限、SSO、OAuth 服务端与 CRUD 脚手架。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Cube?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Cube) |
| [Stardust](https://github.com/NewLifeX/Stardust) | 星尘分布式服务平台：节点管理、APM、配置中心、注册中心与发布中心。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Stardust?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Stardust) |
| [AntJob](https://github.com/NewLifeX/AntJob) | 蚂蚁调度：分布式任务与实时数据平台，支持分片、重试与回溯补算。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AntJob?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AntJob) |
| [NewLife.AI](https://github.com/NewLifeX/NewLife.AI) | 统一接入 46 家大模型服务商，附带开箱即用的 ChatAI Web 对话应用。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.AI?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.AI) |
| **工业与物联网** | | |
| [NewLife.IoT](https://github.com/NewLifeX/NewLife.IoT) | IoT 标准库，定义物联网通信协议契约与设备抽象。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IoT?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IoT) |
| [NewLife.Modbus](https://github.com/NewLifeX/NewLife.Modbus) | Modbus TCP/RTU/ASCII，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Modbus?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Modbus) |
| [NewLife.Siemens](https://github.com/NewLifeX/NewLife.Siemens) | 西门子 PLC 协议，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Siemens?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Siemens) |
| [NewLife.Omron](https://github.com/NewLifeX/NewLife.Omron) | 欧姆龙 PLC 协议，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Omron?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Omron) |
| [NewLife.Melsec](https://github.com/NewLifeX/NewLife.Melsec) | 三菱 Melsec PLC 协议，基于 NewLife.IoT 实现。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Melsec?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Melsec) |
| [NewLife.OPC](https://github.com/NewLifeX/NewLife.OPC) | OPC 客户端，用于工业数据采集。 | — |
| [NewLife.BACnet](https://github.com/NewLifeX/NewLife.BACnet) | BACnet 楼宇自控协议栈。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.BACnet?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.BACnet) |
| [ZeroIoT](https://github.com/NewLifeX/ZeroIoT) | 开源轻量级 IoT 平台，含网关与设备管理。 | — |
| **车联网** | | |
| NewLife.JT808 | JT/T 808 车辆定位协议：70+ 消息体，覆盖 2011/2013/2019 三个版本。 | ![NuGet](https://img.shields.io/nuget/dt/NewLife.JT808?style=flat-square&logo=nuget&label=downloads) |
| NewLife.JT809 | JT/T 809 部标平台互联协议，面向运输管理部门。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.JT809?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.JT809) |
| NewLife.JT1078 | JT/T 1078 车载视频：RTP 解析、FLV/fMP4 转封装与直播推送。 | ![NuGet](https://img.shields.io/nuget/dt/NewLife.JT1078?style=flat-square&logo=nuget&label=downloads) |
| **领域组件** | | |
| [NewLife.Office](https://github.com/NewLifeX/NewLife.Office) | 一个包覆盖 Excel、Word、PPT、PDF 等 15+ 文档格式。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Office?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Office) |
| [NewLife.Audio](https://github.com/NewLifeX/NewLife.Audio) | 跨平台音频：编解码、设备 IO、DSP、容器与流式处理。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Audio?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Audio) |
| [NewLife.Video](https://github.com/NewLifeX/NewLife.Video) | 视频编解码、封装格式、帧处理与流媒体。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Video?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Video) |
| [NewLife.Map](https://github.com/NewLifeX/NewLife.Map) | 百度/高德/腾讯/天地图统一接口，支持地理编码与坐标转换。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.Map?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.Map) |
| [NewLife.IP](https://github.com/NewLifeX/NewLife.IP) | IPv4 归属地：离线内存映射库 + 在线 API 双源。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.IP?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.IP) |
| **工具与产品** | | |
| [CrazyCoder](https://github.com/NewLifeX/XCoder) | 码神工具：网络、串口、加解密、正则、Modbus 与 MQTT。 | [![NuGet](https://img.shields.io/nuget/dt/NewLife.XCoder?style=flat-square&logo=nuget&label=downloads)](https://www.nuget.org/packages/NewLife.XCoder) |
| [GitCandy](https://github.com/NewLifeX/GitCandy) | 自托管 Git 源代码管理系统。 | — |
| [LuckyClover](https://github.com/NewLifeX/LuckyClover) | 从低版本宿主启动时，自动安装匹配的 .NET 运行时。 | — |

> 徽章取各项目主包；主题包、扩展包与卫星包见对应 NuGet 页面。JT808/JT1078 正在发布准备中，包上线后徽章自动显示下载量。未在表中列出的 30 多个包（FTP、LoRa、Office 扩展、各类协议驱动等）见 [NewLifeX 组织](https://github.com/NewLifeX) 与 [newlifex.com](https://newlifex.com)。

## 新生命开发团队

<img src="https://newlifex.com/logo.png" width="48" align="right" alt="NewLife" />

新生命团队成立于 2002 年，二十余年专注 .NET 与物联网技术生态，主导开源项目近百个（生态版图收录 34 个主干项目），为电力、物流、医疗、工控、互联网等行业提供软硬件一体化解决方案；以“学无先后达者为师”为信条。

开源组件在 NuGet 累计下载 <!--nuget-total:start-->6.48M<!--nuget-total:end--> 次（全生态口径，截至 2026-10）。

网站：https://newlifex.com · 开源组织：https://github.com/NewLifeX

<details>
<summary>微信公众号：智能大石头（扫码关注）</summary>

![智能大石头](https://newlifex.com/stone.jpg)

</details>

## 许可证

[MIT](LICENSE) © 2022-2026 新生命开发团队

