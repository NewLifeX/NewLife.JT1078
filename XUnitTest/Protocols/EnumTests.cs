using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Protocols;

public class EnumTests
{
    [Fact(DisplayName = "AVTypes 基本值")]
    public void AVTypes_Values()
    {
        Assert.Equal(6, (Byte)AVTypes.G711A);
        Assert.Equal(7, (Byte)AVTypes.G711U);
        Assert.Equal(26, (Byte)AVTypes.ADPCMA);
        Assert.Equal(19, (Byte)AVTypes.AAC);
        Assert.Equal(25, (Byte)AVTypes.MP3);
        Assert.Equal(91, (Byte)AVTypes.Transparent);
        Assert.Equal(98, (Byte)AVTypes.H264);
        Assert.Equal(99, (Byte)AVTypes.H265);
    }

    [Fact(DisplayName = "PacketFlags 基本值")]
    public void PacketFlags_Values()
    {
        Assert.Equal(0, (Byte)PacketFlags.Alone);
        Assert.Equal(1, (Byte)PacketFlags.First);
        Assert.Equal(2, (Byte)PacketFlags.Last);
        Assert.Equal(3, (Byte)PacketFlags.Middle);
    }

    [Fact(DisplayName = "DataTypes 基本值")]
    public void DataTypes_Values()
    {
        Assert.Equal(0, (Byte)DataTypes.VideoIFrame);
        Assert.Equal(1, (Byte)DataTypes.VideoPFrame);
        Assert.Equal(2, (Byte)DataTypes.VideoBFrame);
        Assert.Equal(3, (Byte)DataTypes.AudioFrame);
        Assert.Equal(4, (Byte)DataTypes.Transparent);
    }

    [Fact(DisplayName = "MessageKinds 协议值")]
    public void MessageKinds_Values()
    {
        Assert.Equal(0x9003, (UInt16)MessageKinds.QueryAVAttribute);
        Assert.Equal(0x1003, (UInt16)MessageKinds.UploadAVAttribute);
        Assert.Equal(0x1005, (UInt16)MessageKinds.UploadPassengerFlow);
        Assert.Equal(0x9101, (UInt16)MessageKinds.AVTransmitRequest);
        Assert.Equal(0x9102, (UInt16)MessageKinds.AVTransmitControl);
        Assert.Equal(0x9105, (UInt16)MessageKinds.AVStatusNotify);
        Assert.Equal(0x9201, (UInt16)MessageKinds.PlaybackRequest);
        Assert.Equal(0x9202, (UInt16)MessageKinds.PlaybackControl);
        Assert.Equal(0x9205, (UInt16)MessageKinds.QueryResourceList);
        Assert.Equal(0x9206, (UInt16)MessageKinds.FileUploadCommand);
        Assert.Equal(0x9207, (UInt16)MessageKinds.FileUploadControl);
        Assert.Equal(0x1205, (UInt16)MessageKinds.AVResourceList);
        Assert.Equal(0x1206, (UInt16)MessageKinds.FileUploadComplete);
    }

    [Fact(DisplayName = "CodeStreams 基本值")]
    public void CodeStreams_Values()
    {
        Assert.Equal(0, (Byte)CodeStreams.Main);
        Assert.Equal(1, (Byte)CodeStreams.Sub);
    }

    [Fact(DisplayName = "MediaKinds 基本值")]
    public void MediaKinds_Values()
    {
        Assert.Equal(0, (Byte)MediaKinds.AudioVideo);
        Assert.Equal(1, (Byte)MediaKinds.Video);
        Assert.Equal(2, (Byte)MediaKinds.TwoWayTalk);
        Assert.Equal(3, (Byte)MediaKinds.Monitor);
        Assert.Equal(4, (Byte)MediaKinds.CentralBroadcast);
    }
}
