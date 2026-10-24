# ChangeLog

## v1.0.2026.1024

### 新增
- 集群命令下发总线与 `DeviceSessionManager` 统一设备会话管理（本地命中 + 广播转发）
- UDP 传输支持、HLS（MPEG-TS）切片器与纯 C# MP3 编码器
- Avalonia 桌面测试客户端：直播拉流、录像回放、设备命令、TCP/UDP 直连，集成 LibVLC 播放
- `ProtocolDecoder` 协议指令解码（T1205/T9003/T9101/T9102/T9201 等）
- NativeAOT 支持：`ILLink.Descriptors.xml` + AotSmoke 冒烟工程 + win-x64/linux-x64/linux-arm64 等 RID 矩阵
- 运行时多语言本地化：三个程序集词表（10 种语言），日志/异常/界面消息按语言翻译
- 11 种语言 README 与生态版图、团队介绍

### 改进
- 升级依赖：NewLife.Core 12.0.2026.1024、NewLife.Audio 1.3.2026.1024、NewLife.Video 1.3.2026.1024
- 适配 NewLife.Core 12 的 `IMessageCodec`/`MessagePump` 协议模式，流式消息体正确物化
- FLV/fMP4 转封装收敛到 NewLife.Video，音频编解码收敛到 NewLife.Audio，核心包只保留 RTP 相关逻辑
- 发布包启用强名称签名、固定程序集版本，附带 snupkg 符号包、SourceLink 与 README.nuget
- 解决方案切换为 slnx，统一 file-scoped 命名空间并英文化枚举成员

### 修复
- 分包合并：缺少首包时丢弃残缺帧、空消息安全返回、原子包不参与合并
- H.264：SPS 解析改用边界检查解析器避免栈溢出；连续起始码不再产生空 NAL 单元
- 解包后分段负载转为连续缓冲区，链式负载可深拷贝与整帧构建
- 会话注销按对象引用匹配，避免重连时误删新会话；下行信令保持会话通道
- 客户端补充应用清单，修复原生视频窗口创建失败

### 测试
- 168 项单元测试（net10.0；net8.0 为 140 项）覆盖协议、编解码、消息模型、服务端、客户端与多语言
- CI：Windows 构建 net461/netstandard/net6.0/net8.0/net10.0 全目标并扫描 IL 裁剪告警，Ubuntu/Windows 双平台测试，pack 资产校验

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
