using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Client.Services;
using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Client;

public class ClientTests
{
    [Fact(DisplayName = "ConnectionConfig 默认值")]
    public void ConnectionConfig_Default()
    {
        var config = new ConnectionConfig();

        Assert.Equal("127.0.0.1", config.Host);
        Assert.Equal(5000, config.HttpPort);
        Assert.Equal(1078, config.TcpPort);
        Assert.Equal(1078, config.UdpPort);
        Assert.Equal("demo", config.Token);
        Assert.Equal("http://127.0.0.1:5000", config.HttpBaseUrl);
        Assert.Equal("ws://127.0.0.1:5000", config.WsBaseUrl);
    }

    [Fact(DisplayName = "MessageBuilder 构建 H264 I帧")]
    public void MessageBuilder_BuildVideoIFrame()
    {
        var msg = MessageBuilder.BuildVideoIFrame("138001380000", 1);

        Assert.NotNull(msg);
        Assert.Equal(AVTypes.H264, msg.Type);
        Assert.Equal(DataTypes.VideoIFrame, msg.DataType);
        Assert.Equal(PacketFlags.Alone, msg.PacketFlag);
        Assert.Equal("138001380000", msg.SIM);
        Assert.Equal(1, msg.Channel);
        Assert.True(msg.Completed);
        Assert.Equal((Byte)129, msg.Version);
    }

    [Fact(DisplayName = "MessageBuilder 构建音频帧")]
    public void MessageBuilder_BuildAudioFrame()
    {
        var msg = MessageBuilder.BuildAudioFrame("138001380000", 2);

        Assert.NotNull(msg);
        Assert.Equal(AVTypes.G711A, msg.Type);
        Assert.Equal(DataTypes.AudioFrame, msg.DataType);
        Assert.Equal((Byte)2, msg.Channel);
    }

    [Fact(DisplayName = "MessageBuilder 自定义构建 JTMessage")]
    public void MessageBuilder_BuildCustom()
    {
        var payload = new Byte[] { 0x01, 0x02, 0x03, 0x04 };
        var msg = MessageBuilder.Build(
            AVTypes.H265,
            DataTypes.VideoPFrame,
            PacketFlags.Alone,
            "138001380000",
            3,
            timestamp: 5000,
            lastFrameInterval: 33,
            payload: payload
        );

        Assert.NotNull(msg);
        Assert.Equal(AVTypes.H265, msg.Type);
        Assert.Equal(DataTypes.VideoPFrame, msg.DataType);
        Assert.Equal(5000UL, msg.Timestamp);
        Assert.Equal(33, msg.LastFrameInterval);
        Assert.NotNull(msg.Payload);
        Assert.Equal(4, msg.Payload.Total);
    }

    [Fact(DisplayName = "MessageBuilder 生成测试 NALU 数据")]
    public void MessageBuilder_GenerateTestNalu()
    {
        var nalu = MessageBuilder.GenerateTestNalu();

        Assert.NotNull(nalu);
        Assert.True(nalu.Length > 0);
        // 应该以起始码 00 00 01 开头
        Assert.Equal(0x00, nalu[0]);
        Assert.Equal(0x00, nalu[1]);
        Assert.Equal(0x01, nalu[2]);
    }

    [Fact(DisplayName = "MessageBuilder 序列化往返")]
    public void MessageBuilder_RoundTrip()
    {
        var original = MessageBuilder.BuildVideoIFrame("138001380000", 1);
        var pk = original.ToPacket();
        Assert.NotNull(pk);

        var parsed = new JTMessage();
        Assert.True(parsed.Read(pk));

        Assert.Equal(original.SIM, parsed.SIM);
        Assert.Equal(original.Channel, parsed.Channel);
        Assert.Equal(original.DataType, parsed.DataType);
        Assert.Equal(original.Type, parsed.Type);
    }

    [Fact(DisplayName = "MessageLogEntry 日志条目")]
    public void MessageLogEntry_Create()
    {
        var entry = new MessageLogEntry
        {
            Direction = "→ 发送",
            Summary = "测试消息",
            HexData = "01A2B3",
            MessageType = "JTMessage",
            Level = "Info",
        };

        Assert.Equal("→ 发送", entry.Direction);
        Assert.Equal("测试消息", entry.Summary);
        Assert.Equal("01A2B3", entry.HexData);
        Assert.Equal("Info", entry.Level);
        // Timestamp 应该是当前时间附近
        Assert.True((DateTime.Now - entry.Timestamp).TotalSeconds < 5);
    }

    [Fact(DisplayName = "StreamStatistics 统计信息")]
    public void StreamStatistics_Create()
    {
        var stats = new StreamStatistics
        {
            ReceivedBytes = 1024,
            ReceivedPackets = 10,
            Bitrate = 8000,
            Duration = TimeSpan.FromSeconds(30),
        };

        Assert.Equal(1024, stats.ReceivedBytes);
        Assert.Equal(10, stats.ReceivedPackets);
        Assert.Equal(8000, stats.Bitrate);
        Assert.Equal(30, stats.Duration.TotalSeconds);
    }

    [Fact(DisplayName = "DeviceInfo 设备信息")]
    public void DeviceInfo_Create()
    {
        var device = new DeviceInfo
        {
            DeviceKey = "138001380000_1",
            Remote = "192.168.1.100:1078",
            Type = "TCP",
            LastActive = DateTime.Now,
        };

        Assert.Equal("138001380000_1", device.DeviceKey);
        Assert.Equal("192.168.1.100:1078", device.Remote);
        Assert.Equal("TCP", device.Type);
    }

    [Fact(DisplayName = "ProtocolDecoder 解码 T9101 实时音视频传输请求")]
    public void ProtocolDecoder_DecodeT9101()
    {
        // T9101 二进制：ServerIP(4) + TcpPort(2) + UdpPort(2) + Channel(1) + MediaKind(1) + CodeStream(1)
        // IP 192.168.1.100, TCP 1078, UDP 1078, CH=1, MediaKind=0(音视频), CodeStream=0(主码流)
        var payload = new Byte[]
        {
            0x91, 0x01, // MessageKind=0x9101
            0xC0, 0xA8, 0x01, 0x64, // 192.168.1.100
            0x04, 0x36, // TCP 1078
            0x04, 0x36, // UDP 1078
            0x01, // Channel
            0x00, // MediaKind
            0x00, // CodeStream
        };
        var decoded = NewLife.JT1078.Client.Services.ProtocolDecoder.Decode(payload);

        Assert.NotNull(decoded);
        Assert.Equal(NewLife.JT1078.Protocols.MessageKinds.AVTransmitRequest, decoded.Kind);
        Assert.True(decoded.IsParsed);
        Assert.Equal("平台请求实时音视频传输", decoded.Description);

        // 验证各字段
        Assert.Contains(decoded.Fields, f => f.Name == "Server" && f.Value == "192.168.1.100");
        Assert.Contains(decoded.Fields, f => f.Name == "TcpPort" && f.Value == "1078");
        Assert.Contains(decoded.Fields, f => f.Name == "UdpPort" && f.Value == "1078");
        Assert.Contains(decoded.Fields, f => f.Name == "Channel" && f.Value == "0x01 (1)");
    }

    [Fact(DisplayName = "ProtocolDecoder 解码 T9201 远程录像回放请求")]
    public void ProtocolDecoder_DecodeT9201()
    {
        // T9201: ServerIP(4) + TcpPort(2) + UdpPort(2) + Channel(1) + FileKind(1) + CodeStream(1) + StorageKind(1) + Mode(1) + Multiple(1) + StartTime(6) + EndTime(6)
        var payload = new Byte[]
        {
            0x92, 0x01, // MessageKind=0x9201
            0xC0, 0xA8, 0x01, 0x64, // 192.168.1.100
            0x04, 0x36, // TCP 1078
            0x04, 0x36, // UDP 1078
            0x01, // Channel
            0x00, // FileKind
            0x00, // CodeStream
            0x00, // StorageKind
            0x00, // Mode
            0x00, // Multiple
            0x24, 0x07, 0x14, 0x15, 0x30, 0x00, // StartTime BCD: 240714153000
            0x24, 0x07, 0x14, 0x16, 0x30, 0x00, // EndTime BCD: 240714163000
        };
        var decoded = NewLife.JT1078.Client.Services.ProtocolDecoder.Decode(payload);

        Assert.NotNull(decoded);
        Assert.Equal(NewLife.JT1078.Protocols.MessageKinds.PlaybackRequest, decoded.Kind);
        Assert.True(decoded.IsParsed);

        // 验证时间解析（BCD → 可读时间）
        var startField = decoded.Fields.FirstOrDefault(f => f.Name == "StartTime");
        Assert.NotNull(startField);
        Assert.Contains("2024-07-14", startField.Value);
    }

    [Fact(DisplayName = "ProtocolDecoder 解码 T9102 音视频实时传输控制")]
    public void ProtocolDecoder_DecodeT9102()
    {
        // T9102: Channel(1) + Command(1) + CloseKind(1) + CodeStream(1)
        var payload = new Byte[]
        {
            0x91, 0x02, // MessageKind=0x9102
            0x01, // Channel
            0x00, // Command=0 (关闭音视频)
            0x00, // CloseKind=0 (关闭音视频数据)
            0x00, // CodeStream
        };
        var decoded = NewLife.JT1078.Client.Services.ProtocolDecoder.Decode(payload);

        Assert.NotNull(decoded);
        Assert.Equal(NewLife.JT1078.Protocols.MessageKinds.AVTransmitControl, decoded.Kind);
        Assert.True(decoded.IsParsed);
        Assert.Equal(4, decoded.Fields.Count);
    }

    [Fact(DisplayName = "ProtocolDecoder 解码 T9003 空体消息")]
    public void ProtocolDecoder_DecodeT9003()
    {
        // T9003 空体消息：只有 MessageKind 头
        var payload = new Byte[] { 0x90, 0x03 };
        var decoded = NewLife.JT1078.Client.Services.ProtocolDecoder.Decode(payload);

        Assert.NotNull(decoded);
        Assert.Equal(NewLife.JT1078.Protocols.MessageKinds.QueryAVAttribute, decoded.Kind);
        Assert.True(decoded.IsParsed);
        Assert.Empty(decoded.Fields);
    }

    [Fact(DisplayName = "ProtocolDecoder 未知消息返回未解析状态")]
    public void ProtocolDecoder_UnknownKind()
    {
        // 未知的 MessageKind
        var payload = new Byte[] { 0xFF, 0xFF, 0x01, 0x02, 0x03 };
        var decoded = NewLife.JT1078.Client.Services.ProtocolDecoder.Decode(payload);

        Assert.NotNull(decoded);
        Assert.False(decoded.IsParsed);
        // 未知类型的 KindName 为数值字符串
        Assert.NotNull(decoded.KindName);
    }

    [Fact(DisplayName = "ProtocolDecoder 空数据返回未解析")]
    public void ProtocolDecoder_EmptyData()
    {
        var decoded = NewLife.JT1078.Client.Services.ProtocolDecoder.Decode([]);

        Assert.NotNull(decoded);
        Assert.False(decoded.IsParsed);
    }

    [Fact(DisplayName = "MessageKindResolver 列出已知类型")]
    public void ProtocolDecoder_GetKnownKinds()
    {
        var kinds = NewLife.JT1078.Client.Services.ProtocolDecoder.GetKnownKinds();

        Assert.NotEmpty(kinds);
        Assert.Contains(NewLife.JT1078.Protocols.MessageKinds.AVTransmitRequest, kinds);
        Assert.Contains(NewLife.JT1078.Protocols.MessageKinds.PlaybackRequest, kinds);
        Assert.Contains(NewLife.JT1078.Protocols.MessageKinds.QueryAVAttribute, kinds);
    }
}
