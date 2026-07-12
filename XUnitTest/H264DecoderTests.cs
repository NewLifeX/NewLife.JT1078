using NewLife.JT1078.H264;
using NewLife.JT1078.Protocols;
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
            Payload = new Packet(nalData),
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
            Payload = new Packet(nalBytes),
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
}
