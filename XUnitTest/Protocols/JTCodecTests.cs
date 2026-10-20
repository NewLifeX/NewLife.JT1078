using System.Buffers;
using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Messaging;
using Xunit;

namespace XUnitTest.Protocols;

/// <summary>JT1078 帧编解码器测试</summary>
public class JTCodecTests
{
    private static JTMessage CreateVideoMessage(Int32 payloadLength = 64)
    {
        var payload = new Byte[payloadLength];
        for (var i = 0; i < payload.Length; i++) payload[i] = (Byte)(i & 0xFF);

        return new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            Sequence = 0x1234,
            SIM = "138001380000",
            Channel = 3,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 0x0102030405060708,
            LastIFrameInterval = 0x0102,
            LastFrameInterval = 0x0304,
            Payload = new ArrayPacket(payload),
        };
    }

    private static JTMessage CreateTransparentMessage(Int32 payloadLength = 8)
    {
        var payload = new Byte[payloadLength];
        for (var i = 0; i < payload.Length; i++) payload[i] = (Byte)(0xA0 + i);

        return new JTMessage
        {
            Version = 129,
            Type = AVTypes.Transparent,
            Completed = true,
            Sequence = 7,
            SIM = "138001380001",
            Channel = 0,
            DataType = DataTypes.Transparent,
            PacketFlag = PacketFlags.Alone,
            Payload = new ArrayPacket(payload),
        };
    }

    private static Byte[] ToBytes(JTMessage msg) => msg.ToPacket().GetSpan().ToArray();

    [Fact(DisplayName = "帧解析：空序列与短头部返回 null")]
    public void Parse_InsufficientHeader_ReturnsNull()
    {
        var codec = new JTCodec();

        Assert.Null(codec.TryParse(ReadOnlySequence<Byte>.Empty));
        Assert.Null(codec.TryParse(new ReadOnlySequence<Byte>(new Byte[15])));
    }

    [Fact(DisplayName = "帧解析：视频帧完整头部产出文件头与体长")]
    public void Parse_VideoHeader()
    {
        var msg = CreateVideoMessage(64);
        var frame = ToBytes(msg);
        var codec = new JTCodec();

        // 头部未到齐
        Assert.Null(codec.TryParse(new ReadOnlySequence<Byte>(frame.AsMemory(0, 29))));

        // 只给头部（30字节），体未到齐也应产出消息头部字段
        var rs = codec.TryParse(new ReadOnlySequence<Byte>(frame.AsMemory(0, 30)));
        Assert.NotNull(rs);

        var result = rs!.Value;
        Assert.False(result.Invalid);
        Assert.Equal(30, result.HeaderSize);
        Assert.Equal(64, result.BodyLength);

        var parsed = Assert.IsType<JTMessage>(result.Message);
        Assert.Equal(msg.Version, parsed.Version);
        Assert.Equal(msg.Type, parsed.Type);
        Assert.Equal(msg.Completed, parsed.Completed);
        Assert.Equal(msg.Sequence, parsed.Sequence);
        Assert.Equal(msg.SIM, parsed.SIM);
        Assert.Equal(msg.Channel, parsed.Channel);
        Assert.Equal(msg.DataType, parsed.DataType);
        Assert.Equal(msg.PacketFlag, parsed.PacketFlag);
        Assert.Equal(msg.Timestamp, parsed.Timestamp);
        Assert.Equal(msg.LastIFrameInterval, parsed.LastIFrameInterval);
        Assert.Equal(msg.LastFrameInterval, parsed.LastFrameInterval);
        Assert.Null(parsed.Payload);
    }

    [Fact(DisplayName = "帧解析：完整帧产出负载")]
    public void Parse_FullFrame()
    {
        var msg = CreateVideoMessage(32);
        var frame = ToBytes(msg);
        var codec = new JTCodec();

        var rs = codec.TryParse(new ReadOnlySequence<Byte>(frame));
        Assert.NotNull(rs);

        var result = rs!.Value;
        Assert.False(result.Invalid);
        Assert.Equal(frame.Length, result.HeaderSize + (Int32)result.BodyLength);
    }

    [Fact(DisplayName = "帧解析：透传帧无时间戳与帧间隔字段")]
    public void Parse_TransparentHeader()
    {
        var msg = CreateTransparentMessage(8);
        var frame = ToBytes(msg);
        var codec = new JTCodec();

        Assert.Null(codec.TryParse(new ReadOnlySequence<Byte>(frame.AsMemory(0, 17))));

        var rs = codec.TryParse(new ReadOnlySequence<Byte>(frame.AsMemory(0, 18)));
        Assert.NotNull(rs);
        Assert.Equal(18, rs!.Value.HeaderSize);
        Assert.Equal(8, rs.Value.BodyLength);
        Assert.Equal(0UL, Assert.IsType<JTMessage>(rs.Value.Message).Timestamp);
    }

    [Fact(DisplayName = "帧解析：魔数错误标记为损坏帧")]
    public void Parse_InvalidMagic()
    {
        var frame = ToBytes(CreateVideoMessage());
        frame[0] = (Byte)'x';
        var codec = new JTCodec();

        var rs = codec.TryParse(new ReadOnlySequence<Byte>(frame));
        Assert.NotNull(rs);
        Assert.True(rs!.Value.Invalid);
        Assert.Null(rs.Value.Message);
    }

    [Fact(DisplayName = "帧解析：超过最大负载长度标记为损坏帧")]
    public void Parse_BodyTooLarge()
    {
        var frame = ToBytes(CreateVideoMessage());
        var codec = new JTCodec { MaxBodyLength = 16 };

        var rs = codec.TryParse(new ReadOnlySequence<Byte>(frame));
        Assert.NotNull(rs);
        Assert.True(rs!.Value.Invalid);
    }

    [Fact(DisplayName = "帧解析：跨段只读序列")]
    public void Parse_MultiSegment()
    {
        var frame = ToBytes(CreateVideoMessage(16));
        var codec = new JTCodec();

        // 在头部中间与体中间分别切段
        var first = new Segment(frame.AsMemory(0, 7));
        var last = first.Append(frame.AsMemory(7, 20)).Append(frame.AsMemory(27));
        var seq = new ReadOnlySequence<Byte>(first, 0, last, last.Memory.Length);

        var rs = codec.TryParse(seq);
        Assert.NotNull(rs);
        Assert.False(rs!.Value.Invalid);
        Assert.Equal(30, rs.Value.HeaderSize);
        Assert.Equal(16, rs.Value.BodyLength);
        Assert.Equal("138001380000", Assert.IsType<JTMessage>(rs.Value.Message).SIM);
    }

    [Fact(DisplayName = "帧构建：Build 与 ToPacket 一致")]
    public void Build_RoundTrip()
    {
        var msg = CreateVideoMessage(48);
        var codec = new JTCodec();

        var pk = codec.Build(msg);
        Assert.NotNull(pk);
        Assert.Equal(ToBytes(msg).Length, pk.Total);

        // 构建结果可再次解析
        var rs = codec.TryParse(new ReadOnlySequence<Byte>(pk.GetSpan().ToArray()));
        Assert.NotNull(rs);
        Assert.Equal(48, rs!.Value.BodyLength);
    }

    [Fact(DisplayName = "帧构建：BuildHeader 声明体长")]
    public void BuildHeader_DeclaresBodyLength()
    {
        var msg = CreateVideoMessage(1234);
        var codec = new JTCodec();

        var pk = codec.BuildHeader(msg, 1234);
        Assert.NotNull(pk);
        Assert.Equal(30, pk.Total);

        var span = pk.GetSpan();
        Assert.Equal(1234, (span[28] << 8) | span[29]);

        // 头部自身可被解析，且体长正确
        var rs = codec.TryParse(new ReadOnlySequence<Byte>(span.ToArray()));
        Assert.NotNull(rs);
        Assert.Equal(30, rs!.Value.HeaderSize);
        Assert.Equal(1234, rs.Value.BodyLength);
    }

    [Fact(DisplayName = "帧构建：BuildHeader 体长超出协议上限")]
    public void BuildHeader_OutOfRange()
    {
        var codec = new JTCodec();

        Assert.Throws<ArgumentOutOfRangeException>(() => codec.BuildHeader(CreateVideoMessage(1), -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => codec.BuildHeader(CreateVideoMessage(1), 65536));
    }

    [Fact(DisplayName = "帧构建：非 JTMessage 抛出参数异常")]
    public void Build_WrongType()
    {
        var codec = new JTCodec();

        Assert.Throws<ArgumentException>(() => codec.Build(null!));
        Assert.Throws<ArgumentException>(() => codec.BuildHeader(new Message(), 0));
    }

    [Fact(DisplayName = "长度探测：GetLength 与 GetLength2 一致")]
    public void GetLength_Consistent()
    {
        var cases = new[]
        {
            CreateVideoMessage(0),
            CreateVideoMessage(1),
            CreateVideoMessage(1000),
            CreateTransparentMessage(3),
        };

        foreach (var msg in cases)
        {
            var data = ToBytes(msg);
            var pk = new ArrayPacket(data);

            Assert.Equal(data.Length, JTMessage.GetLength(pk));
            Assert.Equal(data.Length, JTMessage.GetLength2(data));
        }
    }

    [Fact(DisplayName = "长度探测：数据不足返回 0")]
    public void GetLength_InsufficientData()
    {
        var data = ToBytes(CreateVideoMessage(10));
        var codec = new JTCodec();

        Assert.Equal(0, JTMessage.GetLength(new ArrayPacket(data.AsSpan(0, 16).ToArray())));
        Assert.Equal(0, JTMessage.GetLength2(data.AsSpan(0, 16)));
        Assert.Null(codec.TryParse(new ReadOnlySequence<Byte>(data.AsMemory(0, 16))));
    }

    [Fact(DisplayName = "帧构建：空 SIM 头部构建")]
    public void BuildHeader_NullSim()
    {
        var msg = new JTMessage
        {
            Version = 129,
            Type = AVTypes.H264,
            Completed = true,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1,
        };

        var pk = new JTCodec().BuildHeader(msg, 0);
        Assert.Equal(30, pk.Total);
    }

    /// <summary>只读序列分段</summary>
    private sealed class Segment : ReadOnlySequenceSegment<Byte>
    {
        public Segment(ReadOnlyMemory<Byte> memory) => Memory = memory;

        public Segment Append(ReadOnlyMemory<Byte> memory)
        {
            var segment = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
            Next = segment;

            return segment;
        }
    }
}
