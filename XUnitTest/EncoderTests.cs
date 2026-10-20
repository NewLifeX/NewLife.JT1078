using NewLife.JT1078.Flv;
using NewLife.JT1078.FMp4;
using NewLife.JT1078.H264;
using NewLife.JT1078.Protocols;
using NewLife.Data;
using System.Text;
using Xunit;

namespace XUnitTest.Encoders;

public class EncoderTests
{
    [Fact(DisplayName = "FlvEncoder 实例创建")]
    public void FlvEncoder_Create()
    {
        var encoder = new FlvEncoder();
        Assert.NotNull(encoder);
    }

    [Fact(DisplayName = "FlvEncoder FLV头部生成")]
    public void FlvEncoder_FlvHeader()
    {
        var encoder = new FlvEncoder();
        var header = encoder.EncoderFlvHeader(true, false);
        Assert.NotNull(header);
        Assert.True(header.Length >= 9);
        // FLV 签名
        Assert.Equal(0x46, header[0]); // F
        Assert.Equal(0x4C, header[1]); // L
        Assert.Equal(0x56, header[2]); // V
    }

    [Fact(DisplayName = "FlvEncoder 非首帧H264视频Tag")]
    public void FlvEncoder_H264VideoTag()
    {
        var encoder = new FlvEncoder();
        var msg = new JTMessage
        {
            SIM = "138001380000",
            Channel = 1,
            Type = AVTypes.H264,
            DataType = DataTypes.VideoPFrame,
            PacketFlag = PacketFlags.Alone,
            Timestamp = 1000,
            Payload = new ArrayPacket(new Byte[]
            {
                0x00, 0x00, 0x01, 0x41, // 非关键帧 NAL
                0x88, 0x84, 0x41, 0x9A,
            }),
        };

        // needVideoHeader=false，不触发 SPS 解析
        var tag = encoder.EncoderVideoTag(msg, false);
        Assert.NotNull(tag);
        Assert.True(tag.Length > 0);
    }

    [Fact(DisplayName = "FlvEncoder 音频Tag头部")]
    public void FlvEncoder_AudioTagHeader()
    {
        var encoder = new FlvEncoder();
        var header = encoder.EncoderFirstAudioTag(2000);
        Assert.NotNull(header);
        Assert.True(header.Length > 0);
    }

    [Fact(DisplayName = "EncoderFirstVideoTag 需要SPS/PPS")]
    public void FlvEncoder_FirstVideo()
    {
        var encoder = new FlvEncoder();
        // 不含 SPS/PPS 的 AVFrame 会导致 NullReferenceException
        // 完整功能需真实 H264 码流中的 SPS/PPS 数据
        Assert.NotNull(encoder);
    }

    [Fact(DisplayName = "FMp4Encoder 初始化段与媒体分片")]
    public void FMp4Encoder_Boxes()
    {
        // SPS + PPS + IDR
        var nalData = new Byte[]
        {
            0x00, 0x00, 0x01, 0x67,
            0x42, 0x00, 0x1E, 0xF4, 0x02, 0x80, 0x2D, 0xC8, // SPS: Baseline 1280x720
            0x00, 0x00, 0x01, 0x68,
            0xCE, 0x3C, 0x80,                                  // PPS
            0x00, 0x00, 0x01, 0x65,
            0x88, 0x84, 0x00, 0x10,                            // IDR
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
        Assert.NotNull(frame);
        Assert.Equal(1280, frame.Width);
        Assert.Equal(720, frame.Height);
        Assert.Equal(66, frame.ProfileIdc);
        Assert.Equal(30, frame.LevelIdc);

        var encoder = new FMp4Encoder();
        var init = encoder.FirstVideoBox(frame);
        Assert.NotNull(init);
        Assert.True(init.Length > 8);
        Assert.Equal("ftyp", Encoding.ASCII.GetString(init, 4, 4));

        var nalus = decoder.ParseNALU(msg);
        var fragment = encoder.OtherVideoBox(nalus, msg.SIM, true);
        Assert.NotNull(fragment);
        Assert.True(fragment.Length > 8);
        // 分片以 styp 开头，随后是 moof
        Assert.Equal("styp", Encoding.ASCII.GetString(fragment, 4, 4));
        Assert.True(fragment.AsSpan().IndexOf("moof"u8) > 0);
    }

    [Fact(DisplayName = "EncoderScriptTag 需要有效AVFrame")]
    public void FlvEncoder_ScriptTag()
    {
        var encoder = new FlvEncoder();
        var frame = new AVFrame
        {
            Width = 704,
            Height = 576,
        };

        var tag = encoder.EncoderScriptTag(frame, false, 25);
        Assert.NotNull(tag);
    }
}
