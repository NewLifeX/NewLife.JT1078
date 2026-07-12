using NewLife.JT1078.Protocols;
using NewLife.Data;
using Xunit;

namespace XUnitTest.Protocols;

public class BCDSerializationTests
{
    [Fact(DisplayName = "BCDString 读写 SIM 卡号")]
    public void BCDString_Sim()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
        };

        // 序列化
        var pk = msg.ToPacket();
        Assert.NotNull(pk);

        // 反序列化
        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));

        // BCD 12位编码/解码
        Assert.Equal("138001380000", msg2.SIM);
    }

    [Fact(DisplayName = "BCD 短卡号补零")]
    public void BCDString_ShortSim()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "1380013800", // 10 digits
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
        };

        var pk = msg.ToPacket();
        Assert.NotNull(pk);

        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));

        // 6字节BCD = 12位，10位会被填充
        Assert.Equal(12, msg2.SIM.Length);
    }

    [Fact(DisplayName = "数据包长度 GetLength 计算")]
    public void GetLength_AllTypes()
    {
        // I帧
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            LastIFrameInterval = 0,
            LastFrameInterval = 40,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x67 }),
        };

        var pk = msg.ToPacket();
        var len = JTMessage.GetLength(pk);
        Assert.Equal(pk.Total, len);

        // 音频帧（无时间戳字段）
        msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.G711A,
            Completed = true,
            Sequence = 2,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.AudioFrame,
            PacketFlag = PacketFlags.Alone,
            Payload = new ArrayPacket(new Byte[] { 0x11, 0x22, 0x33, 0x44 }),
        };

        pk = msg.ToPacket();
        len = JTMessage.GetLength(pk);
        Assert.Equal(pk.Total, len);

        // 透传数据（无时间戳、无帧间隔）
        msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.Transparent,
            Completed = true,
            Sequence = 3,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.Transparent,
            PacketFlag = PacketFlags.Alone,
        };

        pk = msg.ToPacket();
        len = JTMessage.GetLength(pk);
        Assert.Equal(pk.Total, len);
    }

    [Fact(DisplayName = "分包合并 First+Last")]
    public void Packet_Append()
    {
        var first = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.First,
            Timestamp = 1000,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x67 }),
        };

        var last = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 2,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Last,
            Timestamp = 1000,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x68 }),
        };

        var complete = first.Append(last);
        Assert.True(complete);
        Assert.Equal(8, first.Payload.Total); // 4+4 bytes
    }

    [Fact(DisplayName = "分包合并 First+Middle+Last")]
    public void Packet_AppendThree()
    {
        var first = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.First,
            Timestamp = 1000,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x67 }),
        };

        var mid = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 2,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Middle,
            Timestamp = 1000,
            Payload = new ArrayPacket(new Byte[] { 0xAA, 0xBB, 0xCC, 0xDD }),
        };

        var last = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 3,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Last,
            Timestamp = 1000,
            Payload = new ArrayPacket(new Byte[] { 0x00, 0x00, 0x01, 0x68 }),
        };

        // First+Middle: 不完整
        var complete = first.Append(mid);
        Assert.False(complete);

        // +Last: 完整
        complete = first.Append(last);
        Assert.True(complete);
        Assert.Equal(12, first.Payload.Total); // 4+4+4 bytes
    }
}
