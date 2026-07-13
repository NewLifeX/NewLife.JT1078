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
}
