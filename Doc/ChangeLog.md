# NewLife.JT1078 变更日志

## v1.0.2026.0713 - 2026-07-13

### 🎉 首版发布

NewLife.JT1078 首个正式版发布，覆盖 JT/T 1078 视频通讯协议全链路。

### ✨ 新功能

- **协议层**：JTMessage RTP 报文解析/构建，JTCodec 粘包编解码，29 种 AVTypes 枚举
- **音频编解码**：G.711A/U、IMA-ADPCM 编解码器，AudioCodecFactory 工厂，FaacEncoder AAC 编码
- **H.264 解析**：H264Decoder NAL 单元分割，SPS/PPS 完整解析，ExpGolombReader
- **FLV 转封装**：FlvEncoder 支持 HTTP-FLV/WebSocket-FLV 实时直播
- **fMP4 转封装**：FMp4Encoder 支持 CMAF 分段输出，49 个核心 ISO BMFF Box 类型
- **TCP 服务端**：NetServer 实现，RTP 分包合并，空闲连接自动断开
- **UDP 服务端**：Socket.ReceiveFrom 实现，与 TCP 双栈共存
- **HTTP/WS 直播**：FLV/fMP4/AAC/PCM/WAV 多格式端点，flv.js 播放器 Demo
- **录像回放**：按 SIM/通道/时间范围的历史音视频回放
- **设备命令总线**：基于 IEventBus 的集群命令下发（单机/Redis 双模式）
- **流媒体授权**：IJT1078Authorization Token 认证
- **DI 构建器**：`AddJT1078Gateway().AddTcp().AddHttp()` 链式注册
- **状态监控**：StatusService 在线设备跟踪、流量统计

### 🧪 测试

- **67 项单元测试**：协议层(30项)、音频编解码(13项)、H.264(4项)、模型(10项)、编码器(6项)、服务端(15项)
- 全部通过

### 📦 NuGet 包

| 包名 | 说明 |
|------|------|
| NewLife.JT1078 | 核心协议 + 编解码 |
| NewLife.JT1078.Flv | FLV 转封装 |
| NewLife.JT1078.FMp4 | fMP4 转封装 |
| NewLife.JT1078.Server | TCP/UDP 服务端 |

### ⚠️ 已知限制

- AAC 编码依赖 libfaac.dll（需自行编译）
- MP3 音频端点/HLS 直播端点为桩代码，需外部编码器库支持
