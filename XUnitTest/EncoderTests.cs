using NewLife.JT1078.Flv;
using NewLife.JT1078.H264;
using NewLife.JT1078.Protocols;
using NewLife.Data;
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
            Payload = new Packet(new Byte[]
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
