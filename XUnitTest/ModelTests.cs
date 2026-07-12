using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Models;

public class ModelTests
{
    [Fact(DisplayName = "T9003 空体消息创建")]
    public void T9003_Create()
    {
        var model = new T9003();
        Assert.NotNull(model);
    }

    [Fact(DisplayName = "T9101 属性设置")]
    public void T9101_Properties()
    {
        var model = new T9101
        {
            Server = "127.0.0.1",
            TcpPort = 1078,
            UdpPort = 1079,
            Channel = 1,
            MediaKind = MediaKinds.AudioVideo,
            CodeStream = CodeStreams.Main,
        };

        Assert.Equal("127.0.0.1", model.Server);
        Assert.Equal(1078, model.TcpPort);
        Assert.Equal(1079, model.UdpPort);
        Assert.Equal(1, model.Channel);
        Assert.Equal(MediaKinds.AudioVideo, model.MediaKind);
        Assert.Equal(CodeStreams.Main, model.CodeStream);
    }

    [Fact(DisplayName = "T9102 属性设置")]
    public void T9102_Properties()
    {
        var model = new T9102
        {
            Channel = 1,
        };

        Assert.Equal(1, model.Channel);
    }

    [Fact(DisplayName = "T9201 属性设置")]
    public void T9201_Properties()
    {
        var model = new T9201
        {
            Server = "127.0.0.1",
            TcpPort = 1078,
            UdpPort = 1079,
            Channel = 1,
            FileKind = MediaFileKinds.AudioVideo,
            CodeStream = CodeStreams.Main,
            StartTime = new DateTime(2026, 7, 12, 8, 0, 0),
            EndTime = new DateTime(2026, 7, 12, 8, 30, 0),
        };

        Assert.Equal("127.0.0.1", model.Server);
        Assert.Equal(MediaFileKinds.AudioVideo, model.FileKind);
        Assert.Equal(CodeStreams.Main, model.CodeStream);
    }

    [Fact(DisplayName = "T9205 属性设置")]
    public void T9205_Properties()
    {
        var model = new T9205
        {
            Channel = 1,
            StartTime = new DateTime(2026, 7, 12, 8, 0, 0),
            EndTime = new DateTime(2026, 7, 12, 8, 30, 0),
            FileKind = MediaFileKinds.AudioVideo,
            CodeStream = CodeStreams.Main,
        };

        Assert.Equal(1, model.Channel);
        Assert.Equal(MediaFileKinds.AudioVideo, model.FileKind);
        Assert.Equal(CodeStreams.Main, model.CodeStream);
    }

    [Fact(DisplayName = "T1005 属性设置")]
    public void T1005_Properties()
    {
        var model = new T1005
        {
            StartTime = new DateTime(2026, 7, 12, 8, 0, 0),
            EndTime = new DateTime(2026, 7, 12, 8, 30, 0),
            GetOnCount = 15,
            GetOffCount = 10,
        };

        Assert.Equal(15, model.GetOnCount);
        Assert.Equal(10, model.GetOffCount);
    }

    [Fact(DisplayName = "T1206 属性设置")]
    public void T1206_Properties()
    {
        var model = new T1206
        {
            SerialNumber = 1,
            Result = 0,
        };

        Assert.Equal(1, model.SerialNumber);
        Assert.Equal(0, model.Result);
    }

    [Fact(DisplayName = "T9105 属性设置")]
    public void T9105_Properties()
    {
        var model = new T9105
        {
            Channel = 1,
            LossRate = 5,
            ErrorRate = 1,
        };

        Assert.Equal(1, model.Channel);
        Assert.Equal(5, model.LossRate);
        Assert.Equal(1, model.ErrorRate);
    }

    [Fact(DisplayName = "T9207 属性设置")]
    public void T9207_Properties()
    {
        var model = new T9207
        {
            SerialNumber = 1,
            Control = 0,
        };

        Assert.Equal(1, model.SerialNumber);
        Assert.Equal(0, model.Control);
    }

    [Fact(DisplayName = "枚举值一致性")]
    public void Enum_Consistency()
    {
        Assert.Equal(0, (Byte)PacketFlags.Alone);
        Assert.Equal(1, (Byte)PacketFlags.First);
        Assert.Equal(2, (Byte)PacketFlags.Last);
        Assert.Equal(3, (Byte)PacketFlags.Middle);

        Assert.Equal(0, (Byte)DataTypes.VideoIFrame);
        Assert.Equal(1, (Byte)DataTypes.VideoPFrame);
        Assert.Equal(2, (Byte)DataTypes.VideoBFrame);
        Assert.Equal(3, (Byte)DataTypes.AudioFrame);
        Assert.Equal(4, (Byte)DataTypes.Transparent);

        Assert.Equal(0, (Byte)CodeStreams.Main);
        Assert.Equal(1, (Byte)CodeStreams.Sub);

        Assert.Equal(0x9003, (UInt16)MessageKinds.QueryAVAttribute);
        Assert.Equal(0x1003, (UInt16)MessageKinds.UploadAVAttribute);
        Assert.Equal(0x9101, (UInt16)MessageKinds.AVTransmitRequest);
        Assert.Equal(0x9102, (UInt16)MessageKinds.AVTransmitControl);
        Assert.Equal(0x9201, (UInt16)MessageKinds.PlaybackRequest);
        Assert.Equal(0x9202, (UInt16)MessageKinds.PlaybackControl);
    }
}
