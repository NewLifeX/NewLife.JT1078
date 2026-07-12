# NewLife.JT1078

![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)
![MIT](https://img.shields.io/badge/license-MIT-green.svg)
![Test](https://github.com/NewLifeX/NewLife.JT1078/workflows/test/badge.svg)

JT/T 1078 道路运输车辆卫星定位系统视频通讯协议的 .NET 实现，覆盖协议解析、音视频编解码、FLV/fMP4 转封装、TCP/UDP 服务端、HTTP/WebSocket 直播推送全链路。

## 功能特性

### 协议层
- **JT1078 RTP 协议**：完整的 RTP 报文解析与构建，支持 29 种音视频编码类型
- **TCP 粘包处理**：基于 PacketCodec 的自动分包/合包
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

## 快速开始

### 安装 NuGet 包

```bash
dotnet add package NewLife.JT1078           # 核心协议库
dotnet add package NewLife.JT1078.Flv       # FLV 转封装
dotnet add package NewLife.JT1078.FMp4      # fMP4 转封装
dotnet add package NewLife.JT1078.Server    # TCP 服务端
```

### 解析 RTP 报文

```csharp
using NewLife.JT1078.Protocols;

// 从网络流解析
var msg = new JTMessage();
msg.Read(stream);

// 检查载荷类型
if (msg.AVType == AVTypes.H264)
{
    // 处理 H.264 视频
    var decoder = new H264Decoder();
    var frame = decoder.ParseAVFrame(msg);
}
```

### 音频解码

```csharp
using NewLife.JT1078.Audio;

var factory = new AudioCodecFactory();
// ADPCM → PCM
var pcm = factory.ToPcm(adpcmData, AVTypes.ADPCMA, state);
// PCM → G.711A
var g711a = factory.FromPcm(pcmData, AVTypes.G711A, null);
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
| NewLife.JT1078 | 核心 RTP 协议 + 音视频编解码 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Flv | FLV 转封装 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Flv.svg)](https://www.nuget.org/packages/NewLife.JT1078.Flv) |
| NewLife.JT1078.FMp4 | fMP4/CMAF 转封装 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.FMp4.svg)](https://www.nuget.org/packages/NewLife.JT1078.FMp4) |
| NewLife.JT1078.Server | TCP/UDP 服务端 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

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
    ├──► NewLife.JT1078.Flv ───► HTTP-FLV / WS-FLV 直播
    ├──► NewLife.JT1078.FMp4 ──► fMP4 直播
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
- `POST /Command/Send` — 向设备发送信令（自动路由到设备所在服务器）
- `GET /Command/Devices` — 查询本地已连接的设备列表

## 直播端点

| 端点 | 格式 | 传输 | 播放器 |
|------|------|------|--------|
| `/live.flv` | FLV | HTTP / WebSocket | flv.js |
| `/live.mp4` | fMP4 | HTTP / WebSocket | 原生 MSE |
| `/live.aac` | AAC | HTTP | 原生音频 |
| `/live.pcm` | PCM | HTTP | 原生音频 |
| `/live.wav` | WAV | HTTP | 原生音频 |

## 协议参考

- JT/T 1078-2016《道路运输车辆卫星定位系统 视频通讯协议》
- ISO 14496-12 (ISO BMFF)
- Adobe Flash Video (FLV)

## 致谢

本项目在设计和实现过程中参考了 [SmallChi/JT1078](https://github.com/SmallChi/JT1078) 开源项目，特此致谢。

## 许可证

[MIT](LICENSE) © 2022-2026 新生命开发团队
