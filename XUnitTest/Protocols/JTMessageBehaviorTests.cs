using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Messaging;
using Xunit;

namespace XUnitTest.Protocols;

/// <summary>JTMessage 消息行为测试</summary>
public class JTMessageBehaviorTests
{
    private static JTMessage CreateMessage(PacketFlags flag, Byte[] payload) => new()
    {
        Version = 129,
        Type = AVTypes.H264,
        Completed = true,
        Sequence = 1,
        SIM = "138001380000",
        Channel = 1,
        DataType = DataTypes.VideoIFrame,
        PacketFlag = flag,
        Timestamp = 100,
        Payload = new ArrayPacket(payload),
    };

    [Fact(DisplayName = "分包合并：首包+中间包+末包")]
    public void Append_FirstMiddleLast()
    {
        var first = CreateMessage(PacketFlags.First, [1, 2]);
        var middle = CreateMessage(PacketFlags.Middle, [3, 4]);
        var last = CreateMessage(PacketFlags.Last, [5, 6]);

        Assert.False(first.Append(middle));
        Assert.Equal(4, first.Payload.Total);

        Assert.True(first.Append(last));
        Assert.Equal(6, first.Payload.Total);
        Assert.Equal(new Byte[] { 1, 2, 3, 4, 5, 6 }, first.Payload.ToArray());
    }

    [Fact(DisplayName = "分包合并：原子包不可合并")]
    public void Append_Alone()
    {
        var first = CreateMessage(PacketFlags.First, [1]);
        var alone = CreateMessage(PacketFlags.Alone, [2]);

        Assert.False(first.Append(alone));
    }

    [Fact(DisplayName = "分包合并：非首包不能合并其它包")]
    public void Append_NeedFirstPacket()
    {
        var last = CreateMessage(PacketFlags.Last, [1]);
        var middle = CreateMessage(PacketFlags.Middle, [2]);

        Assert.Throws<Exception>(() => last.Append(middle));
    }

    [Fact(DisplayName = "创建响应：继承版本与类型")]
    public void CreateReply_InheritsFields()
    {
        var msg = CreateMessage(PacketFlags.Alone, [1, 2, 3]);
        msg.Reply = false;

        var reply = Assert.IsType<JTMessage>(msg.CreateReply());
        Assert.True(reply.Reply);
        Assert.Equal(NewLife.Messaging.MessageKinds.Response, reply.Kind);
        Assert.Equal(msg.Version, reply.Version);
        Assert.Equal(msg.Type, reply.Type);
        Assert.Equal(msg.SIM, reply.SIM);
        Assert.Equal(msg.Channel, reply.Channel);
        Assert.Null(reply.Payload);
    }

    [Fact(DisplayName = "单向请求标记")]
    public void OneWay_SetsKind()
    {
        var msg = CreateMessage(PacketFlags.Alone, [1]);
        msg.OneWay = true;

        Assert.Equal(NewLife.Messaging.MessageKinds.OneWay, msg.Kind);
        Assert.False(msg.Reply);
        Assert.True(msg.OneWay);
    }

    [Fact(DisplayName = "空负载往返")]
    public void EmptyPayload_RoundTrip()
    {
        var msg = CreateMessage(PacketFlags.Alone, []);
        var pk = msg.ToPacket();
        Assert.NotNull(pk);

        var msg2 = new JTMessage();
        Assert.True(msg2.Read(pk));
        Assert.Null(msg2.Payload);
    }

    [Fact(DisplayName = "负载长度超出实际数据时截断读取")]
    public void Read_TruncatedPayload()
    {
        var frame = CreateMessage(PacketFlags.Alone, [9, 8, 7, 6]).ToPacket().GetSpan().ToArray();

        // 篡改长度字段为实际数据的两倍，读取时应截断
        frame[28] = 0;
        frame[29] = 8;

        var msg = new JTMessage();
        Assert.True(msg.Read(new ArrayPacket(frame)));
        Assert.Equal(4, msg.Payload.Total);
    }

    [Fact(DisplayName = "消息键为SIM与通道组合")]
    public void GetKey_CombinesSimAndChannel()
    {
        var msg = CreateMessage(PacketFlags.Alone, [1]);
        Assert.Equal("138001380000_1", msg.GetKey());
    }

    [Fact(DisplayName = "最大序号与最大帧间隔往返")]
    public void MaxValues_RoundTrip()
    {
        var msg = CreateMessage(PacketFlags.Alone, [1, 2, 3]);
        msg.Sequence = UInt16.MaxValue;
        msg.LastIFrameInterval = UInt16.MaxValue;
        msg.LastFrameInterval = UInt16.MaxValue;

        var msg2 = new JTMessage();
        Assert.True(msg2.Read(msg.ToPacket()));

        Assert.Equal(UInt16.MaxValue, msg2.Sequence);
        Assert.Equal(UInt16.MaxValue, msg2.LastIFrameInterval);
        Assert.Equal(UInt16.MaxValue, msg2.LastFrameInterval);
    }

    [Fact(DisplayName = "ToString 包含类型与通道")]
    public void ToString_ContainsKeyFields()
    {
        var msg = CreateMessage(PacketFlags.Alone, [1]);

        var text = msg.ToString();
        Assert.Contains("138001380000", text);
        Assert.Contains("Seq=1", text);
    }
}
