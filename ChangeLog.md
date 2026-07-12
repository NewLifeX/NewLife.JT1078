# ChangeLog

## v1.0.2026.0712

### 新增
- 完整的 JT/T 1078 RTP 协议解析与构建（JTMessage + JTCodec）
- 音频编解码：G.711A、G.711U、ADPCM（含海思芯片头处理）
- H.264 NAL 单元解析、SPS/PPS 提取、Exp-Golomb 解码
- FLV 转封装（HTTP-FLV 直播）
- fMP4/CMAF 转封装（ISOBMFF Box 体系）
- TCP 1078 服务端（基于 NewLife.Net，支持分包合并）
- 12 种 JT1078 消息体模型（T9003/T1003/T1005/T1205/T1206/T9101/T9102/T9105/T9201/T9202/T9205/T9206/T9207）
- JT1078 协议枚举体系（29 种音视频编码类型、消息类型、数据类型、分包标记等）
- 单元测试：JTMessage 序列化往返、数据包长度计算
- 完整的项目文档体系：需求文档、架构设计、功能模块清单
- MIT 开源许可证
- GitHub Actions CI/CD（构建、测试、发布）

### 改进
- 代码风格全面统一：中文标识符 → 英文、C# 别名 → 正式名
- 多目标框架支持：netstandard2.0/2.1、net45/461、net6.0/8.0
- 实现 G711U 编码方向（FromPcm），补全编解码器
- 适配 NewLife.Core 最新 IMessage 接口

### 流媒体服务（新增）
- HTTP/WebSocket 直播端点（FLV/fMP4/PCM/AAC/WAV）
- FLV/fMP4 录像回放端点
- 格式处理器链：FlvHandler/Mp4Handler/AacHandler/PcmHandler/WavHandler
- MsgHub 消息通道（发布/订阅模型）
- SessionManager 订阅者管理
- StatusService 设备在线状态跟踪
- FileCache 录制文件缓存
- ASP.NET Core 集成（NET6.0+）
- 基于 flv.js 的 Web 播放器 Demo

### 测试
- 34 项单元测试覆盖协议、编解码、消息模型、服务端
