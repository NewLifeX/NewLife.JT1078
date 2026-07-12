using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Protocols;

public class JTMessageTests
{
    [Fact(DisplayName = "JTMessage 基本写入读取往返")]
    public void WriteReadRoundTrip()
    {
        var msg = new JTMessage
        {
            Version = 0x41,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            LastIFrameInterval = 0,
            LastFrameInterval = 0,
        };

        // 写入
        var ms = new MemoryStream();
        var rs = msg.Write(ms, null!);
        Assert.True(rs);

        // 读取
        ms.Position = 0;
        var msg2 = new JTMessage();
        rs = msg2.Read(ms, null!);
        Assert.True(rs);

        Assert.Equal(msg.Version, msg2.Version);
        Assert.Equal(msg.Type, msg2.Type);
        Assert.Equal(msg.Completed, msg2.Completed);
        Assert.Equal(msg.Sequence, msg2.Sequence);
        Assert.Equal(msg.SIM, msg2.SIM);
        Assert.Equal(msg.Channel, msg2.Channel);
        Assert.Equal(msg.DataType, msg2.DataType);
        Assert.Equal(msg.PacketFlag, msg2.PacketFlag);
        Assert.Equal(msg.Timestamp, msg2.Timestamp);
    }

    [Fact(DisplayName = "JTMessage 转数据包往返")]
    public void ToPacketRoundTrip()
    {
        var msg = new JTMessage
        {
            Version = 0x41,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
        };

        var pk = msg.ToPacket();
        Assert.NotNull(pk);

        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));

        Assert.Equal(msg.SIM, msg2.SIM);
        Assert.Equal(msg.Channel, msg2.Channel);
        Assert.Equal(msg.DataType, msg2.DataType);
        Assert.Equal(msg.Timestamp, msg2.Timestamp);
    }

    [Fact(DisplayName = "JTMessage 数据包长度计算")]
    public void GetLength()
    {
        var msg = new JTMessage
        {
            Version = 0x41,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 1,
            SIM = "138001380000",
            Channel = 1,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
        };

        var pk = msg.ToPacket();
        Assert.NotNull(pk);

        var len = JTMessage.GetLength(pk);
        Assert.True(len > 0);
        Assert.Equal(pk.Total, len);
    }
}
