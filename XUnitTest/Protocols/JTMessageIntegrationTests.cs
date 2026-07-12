using NewLife.JT1078.Protocols;
using NewLife.Data;
using Xunit;

namespace XUnitTest.Protocols;

public class JTMessageIntegrationTests
{
    /// <summary>构造一个完整的 RTP 报文并验证序列化一致性</summary>
    [Fact(DisplayName = "H264 视频帧完整序列化往返")]
    public void H264Frame_RoundTrip()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 12345,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 12345678,
            LastIFrameInterval = 5000,
            LastFrameInterval = 40,
            Payload = new ArrayPacket(new Byte[]
            {
                0x00, 0x00, 0x01, 0x67, 0x42, 0x00, 0x1E, 0x8D,
                0x00, 0x00, 0x01, 0x68, 0xEB, 0xE3, 0xCB, 0x22, 0xC0,
            }),
        };

        var pk = msg.ToPacket();
        Assert.NotNull(pk);

        var msg2 = new JTMessage();
        var ok = msg2.Read(pk);
        Assert.True(ok);

        Assert.Equal(msg.Version, msg2.Version);
        Assert.Equal(msg.Type, msg2.Type);
        Assert.Equal(msg.Completed, msg2.Completed);
        Assert.Equal(msg.Sequence, msg2.Sequence);
        Assert.Equal(msg.SIM, msg2.SIM);
        Assert.Equal(msg.Channel, msg2.Channel);
        Assert.Equal(msg.DataType, msg2.DataType);
        Assert.Equal(msg.PacketFlag, msg2.PacketFlag);
        Assert.Equal(msg.Timestamp, msg2.Timestamp);
        Assert.Equal(msg.LastIFrameInterval, msg2.LastIFrameInterval);
        Assert.Equal(msg.LastFrameInterval, msg2.LastFrameInterval);
        Assert.Equal(msg.Payload.Total, msg2.Payload.Total);
    }

    [Fact(DisplayName = "G711A 音频帧序列化往返")]
    public void AudioFrame_RoundTrip()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.G711A,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.AudioFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 2000,
            Payload = new ArrayPacket(new Byte[] { 0xD5, 0x55, 0xD5, 0x55 }),
        };

        var pk = msg.ToPacket();
        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));

        Assert.Equal(msg.Type, msg2.Type);
        Assert.Equal(msg.DataType, msg2.DataType);
        Assert.Equal(msg.Timestamp, msg2.Timestamp);
    }

    [Fact(DisplayName = "透传数据帧序列化（无时间戳）")]
    public void TransparentFrame_RoundTrip()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.Transparent,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.Transparent,
            PacketFlag = PacketFlags.Alone,
            // 透传数据没有时间戳字段
            Payload = new ArrayPacket(new Byte[] { 0x01, 0x02, 0x03, 0x04 }),
        };

        var pk = msg.ToPacket();
        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));

        Assert.Equal(DataTypes.Transparent, msg2.DataType);
        // 透传数据时间戳应为 0
        Assert.Equal(0UL, msg2.Timestamp);
    }

    [Fact(DisplayName = "P帧和B帧序列化（含帧间隔）")]
    public void PFrame_RoundTrip()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoPFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1500,
            LastFrameInterval = 33,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x41, 0x88 }),
        };

        var pk = msg.ToPacket();
        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));

        Assert.Equal(DataTypes.VideoPFrame, msg2.DataType);
        Assert.Equal(msg.LastFrameInterval, msg2.LastFrameInterval);
    }

    [Fact(DisplayName = "二进制数据与 Packet 一致性")]
    public void Binary_Consistency()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 65535,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 12345678,
            LastIFrameInterval = 5000,
            LastFrameInterval = 40,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x67 }),
        };

        var pk = msg.ToPacket();
        var total = pk.Total;

        // 通过 GetLength 探测长度
        var len = JTMessage.GetLength(pk);
        Assert.Equal(total, len);
    }
}
