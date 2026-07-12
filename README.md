# NewLife.JT1078

![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)
![MIT](https://img.shields.io/badge/license-MIT-green.svg)

JT/T 1078 道路运输车辆卫星定位系统视频通讯协议的 .NET 实现。

## 功能特性

- **JT1078 RTP 协议**：完整的 RTP 报文解析与构建，支持所有音视频编码类型
- **音视频编解码**：G.711A/G.711U/ADPCM 音频编解码器，H.264 NAL 单元解析
- **FLV 转封装**：将 JT1078 RTP 音视频流实时转为 FLV 格式（HTTP-FLV 直播）
- **fMP4 转封装**：将 JT1078 RTP 音视频流转为 CMAF/fMP4 格式（HLS/DASH 点播直播）
- **TCP 服务端**：开箱即用的 JT1078 TCP 服务器，支持 RTP 分包合并
- **高性能设计**：Span\<T\> 零分配解析、ArrayPool 缓冲区池化
- **跨平台**：支持 .NET Framework 4.5+ / .NET Standard 2.0+ / .NET 6+

## 快速开始

### 安装 NuGet 包

```bash
dotnet add package NewLife.JT1078       # 核心协议库
dotnet add package NewLife.JT1078.Flv  # FLV 转封装
dotnet add package NewLife.JT1078.FMp4 # fMP4 转封装
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

## NuGet 包列表

| 包名 | 说明 | NuGet |
|------|------|-------|
| NewLife.JT1078 | 核心 RTP 协议 + 音视频编解码 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.svg)](https://www.nuget.org/packages/NewLife.JT1078) |
| NewLife.JT1078.Flv | FLV 转封装 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Flv.svg)](https://www.nuget.org/packages/NewLife.JT1078.Flv) |
| NewLife.JT1078.FMp4 | fMP4/CMAF 转封装 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.FMp4.svg)](https://www.nuget.org/packages/NewLife.JT1078.FMp4) |
| NewLife.JT1078.Server | TCP 服务端 | [![NuGet](https://img.shields.io/nuget/v/NewLife.JT1078.Server.svg)](https://www.nuget.org/packages/NewLife.JT1078.Server) |

## 架构

```
JT1078 终端设备
    │  TCP/1078
    ▼
NewLife.JT1078.Server ─── JT1078Session
    │  RTP 分包合并
    ▼
JTMessage (RTP 报文)
    ├──► H264Decoder (视频)
    ├──► AudioCodecFactory (音频)
    │
    ├──► NewLife.JT1078.Flv ──► HTTP-FLV 直播
    ├──► NewLife.JT1078.FMp4 ──► HLS/DASH
    ▼
 播放器
```

## 协议参考

- JT/T 1078-2016《道路运输车辆卫星定位系统 视频通讯协议》

## 致谢

本项目在设计和实现过程中参考了 [SmallChi/JT1078](https://github.com/SmallChi/JT1078) 开源项目，特此致谢。

## 许可证

[MIT](LICENSE) © 2022-2026 新生命开发团队
