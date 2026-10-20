using NewLife.JT1078.H264;
using NewLife.JT1078.Protocols;
using NewLife.Video.Codecs;
using NewLife.Data;
using Xunit;

namespace XUnitTest.H264;

public class H264DecoderTests
{
    [Fact(DisplayName = "H264Decoder ParseNALU 解析NAL单元")]
    public void ParseNALU_Test()
    {
        // 构造含 SPS+PPS NAL 单元的 JTMessage
        var nalData = new Byte[]
        {
            0x00, 0x00, 0x01, 0x67, // SPS 起始码 + NAL头
            0x42, 0x00, 0x1E,       // profile=66, constraint=0, level=30
            0x8D, 0x40, 0x28, 0x02,
            0xE0, 0x3C, 0x00, 0x00, 0x03, 0x00, 0x04, 0x00, 0x00, 0x03,
            0x00, 0xCA, 0x3C, 0x38, 0x1A, 0x0E, 0x3C, 0x18,
            0x00, 0x00, 0x01, 0x68, // PPS 起始码 + NAL头
            0xEB, 0xE3, 0xCB, 0x22, 0xC0,
        };

        var msg = new JTMessage
        {
            SIM = "138001380000",
            Channel = 1,
            Type = AVTypes.H264,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            Payload = new ArrayPacket(nalData),
        };

        var decoder = new H264Decoder();
        var nalus = decoder.ParseNALU(msg);

        Assert.NotNull(nalus);
        Assert.True(nalus.Count >= 2, $"期望至少2个NAL单元，实际{nalus.Count}");

        // 验证 SPS
        var sps = nalus.FirstOrDefault(n => n.NALUHeader.NalUnitType == NalUnitType.SPS);
        Assert.NotNull(sps);
        Assert.Equal(7, (Byte)sps.NALUHeader.NalUnitType);

        // 验证 PPS
        var pps = nalus.FirstOrDefault(n => n.NALUHeader.NalUnitType == NalUnitType.PPS);
        Assert.NotNull(pps);
        Assert.Equal(8, (Byte)pps.NALUHeader.NalUnitType);
    }

    [Fact(DisplayName = "NAL 单元头解析")]
    public void NALUHeader_Parse()
    {
        var header = new NALUHeader(0x67);
        Assert.Equal(0, header.ForbiddenZeroBit);
        Assert.Equal(3, header.NalRefIdc);
        Assert.Equal(NalUnitType.SPS, header.NalUnitType);

        header = new NALUHeader(0x41);
        Assert.Equal(0, header.ForbiddenZeroBit);
        Assert.Equal(NalUnitType.SLICE, header.NalUnitType);

        header = new NALUHeader(0x05);
        Assert.Equal(0, header.ForbiddenZeroBit);
        Assert.Equal(NalUnitType.IDR, header.NalUnitType);
        Assert.True(header.KeyFrame);
    }

    [Fact(DisplayName = "竞争字节剥离")]
    public void EmulationPrevention_Test()
    {
        var decoder = new H264Decoder();
        var nalBytes = new Byte[]
        {
            0x00, 0x00, 0x00, 0x01, 0x67,
            0x00, 0x00, 0x03, 0x00, 0x00, 0x03, 0x01,
        };

        var msg = new JTMessage
        {
            SIM = "138001380000",
            Channel = 1,
            Type = AVTypes.H264,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Payload = new ArrayPacket(nalBytes),
        };

        var nalus = decoder.ParseNALU(msg);
        Assert.NotNull(nalus);
        Assert.NotEmpty(nalus);
    }

    [Fact(DisplayName = "ExpGolombReader 构造")]
    public void ExpGolombReader_Test()
    {
        var bits = new Byte[] { 0xA0, 0x00 };
        var reader = new ExpGolombReader(bits);
        // ref struct 仅编译验证
        _ = reader;
    }

    [Fact(DisplayName = "H264SpsParser 解析基线SPS分辨率")]
    public void SpsParser_Baseline()
    {
        // Baseline 1280x720
        var sps = new Byte[] { 0x67, 0x42, 0x00, 0x1E, 0xF4, 0x02, 0x80, 0x2D, 0xC8 };

        Assert.True(H264SpsParser.TryParse(sps, out var info));
        Assert.Equal(1280, info.width);
        Assert.Equal(720, info.height);
        Assert.Equal(66, info.profileIdc);
        Assert.Equal(30, info.levelIdc);
    }

    [Fact(DisplayName = "H264SpsParser 解析高档SPS")]
    public void SpsParser_HighProfile()
    {
        // High 10 1280x720
        var sps = new Byte[] { 0x67, 0x64, 0x00, 0x1F, 0xA4, 0x8E, 0x80, 0x50, 0x05, 0xB9 };

        Assert.True(H264SpsParser.TryParse(sps, out var info));
        Assert.Equal(1280, info.width);
        Assert.Equal(720, info.height);
        Assert.Equal(100, info.profileIdc);
    }

    [Fact(DisplayName = "H264SpsParser 拒绝截断SPS而不崩溃")]
    public void SpsParser_Invalid()
    {
        // 截断的SPS在 ExpGolombReader 中会无限递归导致栈溢出
        var truncated = new Byte[] { 0x67, 0x42, 0x00, 0x1E, 0x8D, 0x40, 0x28, 0x02, 0xE0, 0x3C };
        Assert.False(H264SpsParser.TryParse(truncated, out _));
        Assert.False(H264SpsParser.TryParse(new Byte[] { 0x67, 0x42 }, out _));
        Assert.False(H264SpsParser.TryParse([], out _));
    }

    [Fact(DisplayName = "ParseAVFrame 遇到截断SPS不崩溃")]
    public void ParseAVFrame_TruncatedSps()
    {
        var nalData = new Byte[]
        {
            0x00, 0x00, 0x01, 0x67,
            0x42, 0x00, 0x1E, 0x8D, 0x40, 0x28, 0x02, 0xE0, 0x3C,
        };
        var msg = new JTMessage
        {
            SIM = "138001380000",
            Channel = 1,
            Type = AVTypes.H264,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            Payload = new ArrayPacket(nalData),
        };

        var frame = new H264Decoder().ParseAVFrame(msg);

        Assert.NotNull(frame);
        Assert.NotNull(frame.SPS);
        Assert.Equal(0, frame.Width);
        Assert.Equal(0, frame.Height);
    }

    [Fact(DisplayName = "ParseAVFrame 支持链式分段负载")]
    public void ParseAVFrame_ChainedPayload()
    {
        var data = new Byte[]
        {
            0x00, 0x00, 0x01, 0x67,
            0x42, 0x00, 0x1E, 0xF4, 0x02, 0x80, 0x2D, 0xC8,
            0x00, 0x00, 0x01, 0x68,
            0xCE, 0x3C, 0x80,
            0x00, 0x00, 0x01, 0x65,
            0x88, 0x84, 0x00, 0x10,
        };
        // 模拟分包合并后的分段链，切点落在起始码中间
        var payload = new ArrayPacket(data.AsSpan(0, 6).ToArray())
            .Append(new ArrayPacket(data.AsSpan(6, 7).ToArray()))
            .Append(new ArrayPacket(data.AsSpan(13).ToArray()));

        // 链式负载首段不包含全部数据，验证按连续缓冲区解析
        Assert.True(payload.Total > payload.GetSpan().Length);

        var msg = new JTMessage
        {
            SIM = "138001380000",
            Channel = 1,
            Type = AVTypes.H264,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            Payload = payload,
        };

        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);

        Assert.Equal(1280, frame.Width);
        Assert.Equal(720, frame.Height);
        Assert.Equal(3, frame.Nalus.Count);
        Assert.Equal(3, decoder.ParseNALU(msg).Count);
    }

    [Fact(DisplayName = "连续起始码不生成空NAL单元")]
    public void ParseAVFrame_ConsecutiveStartCodes()
    {
        var nalData = new Byte[]
        {
            0x00, 0x00, 0x01,
            0x00, 0x00, 0x01,
            0x65, 0x88, 0x84,
        };
        var msg = new JTMessage
        {
            SIM = "138001380000",
            Channel = 1,
            Type = AVTypes.H264,
            DataType = DataTypes.VideoIFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            Payload = new ArrayPacket(nalData),
        };

        var decoder = new H264Decoder();
        var frame = decoder.ParseAVFrame(msg);

        Assert.Single(frame.Nalus);
        Assert.Single(decoder.ParseNALU(msg));
    }

    [Fact(DisplayName = "DiscardEmulationPreventionBytes 剔除防竞争字节")]
    public void DiscardEmulationPreventionBytes_Test()
    {
        var raw = new Byte[] { 0x67, 0x00, 0x00, 0x03, 0x01, 0x00, 0x00, 0x03, 0x02 };

        var result = new H264Decoder().DiscardEmulationPreventionBytes(raw);

        Assert.Equal(new Byte[] { 0x67, 0x00, 0x00, 0x01, 0x00, 0x00, 0x02 }, result);
    }
}
