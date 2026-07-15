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
| NewLife.JT1078 | 核心协议 + 编解码 + FLV/fMP4 转封装 |

### 🔄 架构重构

- **合并 Flv/FMp4 到核心库**：`NewLife.JT1078.Flv` 和 `NewLife.JT1078.FMp4` 两个独立项目合并到核心项目，减少项目数量
- **引入 NewLife.Audio/Video 包**：用 NuGet beta 包替代本地项目引用，音频编解码和视频容器由外部统一提供
- **H264 代码去重**：`ExpGolombReader`/`NALUHeader` 统一使用 `NewLife.Video.Codecs`，消除两份重复代码
- **升级依赖链**：Core/Audio/Video 统一使用 beta 版本，依赖链一致
- **Server/Web 不再发布 NuGet**：仅 `NewLife.JT1078` 核心包发布到 NuGet.org

### ⚠️ 已知限制

- AAC 编码依赖 libfaac.dll（需自行编译）

## v1.0.2026.0715 - 2026-07-15

### 🔧 审计修复

#### P0 数据流修复

- **M8 流媒体直播数据流连通**：MsgHub.ProduceAsync 按 RTPType 分组用户，延迟创建对应 Handler 并调用 EncodeAsync；新增 MsgHubRegistry 静态注册表；JT1078Session.OnMessage 分发到中枢；LiveController 注册 HttpSession 到中枢。直播管线从"端点可连接但无数据"变为完整运行
- **M9 录像回放数据流修复**：FileCache 新增 ReadFile/OpenRead 方法；RecordController 全面重写，从 FileCache 读取录制文件并流式返回 HTTP 响应，新增 RecordModel（SIM/Channel/Date），移除长连接等待

#### P1 废弃 API

- **M7-5 废弃 API 迁移**：JTMessage 新增 GetLength2(ReadOnlySpan<Byte>) 方法，性能更优；JTCodec 和 TcpClientService 切换为 GetLength2 委托，消除 CS0618 警告

#### P2 代码质量

- **M14-8 废弃 API 修复**：3 个 AXAML 文件中 TextBox.Watermark 替换为 PlaceholderText
- **M15-7 测试增强**：Session_SendJT 从 Assert.True(true) 改为方法签名验证

### 🧪 测试

| 指标 | 数值 |
|------|------|
| 测试总数 | **83 项**（+16 项自首版） |
| 测试覆盖率 | M1~M17 全部覆盖 |
| 审计项 | 22 项缺口已修复 20 项，2 项暂缓（外部依赖）
