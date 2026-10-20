using NewLife.JT1078.Models;
using Xunit;

namespace XUnitTest.Protocols;

/// <summary>消息体模型序列化测试</summary>
public class ModelSerializationTests
{
    [Fact(DisplayName = "AVChannelParameter 往返序列化")]
    public void AVChannelParameter_RoundTrip()
    {
        var parameter = new AVChannelParameter
        {
            Items =
            [
                new AVChannelParameterItem { ChennalId = 1, Pasrameter = new AVParameter() },
                new AVChannelParameterItem { ChennalId = 2, Pasrameter = new AVParameter() },
            ],
        };

        var ms = new MemoryStream();
        Assert.True(parameter.Write(ms, null!));
        Assert.Equal(1 + 21 * 2, ms.Length);

        ms.Position = 0;
        var parameter2 = new AVChannelParameter();
        Assert.True(parameter2.Read(ms, null!));

        Assert.Equal(2, parameter2.Count);
        Assert.Equal(2, parameter2.Items.Length);
        Assert.Equal(1, parameter2.Items[0].ChennalId);
        Assert.Equal(2, parameter2.Items[1].ChennalId);
        Assert.NotNull(parameter2.Items[0].Pasrameter);
    }

    [Fact(DisplayName = "AVChannelParameter 空列表序列化")]
    public void AVChannelParameter_Empty()
    {
        var parameter = new AVChannelParameter();

        var ms = new MemoryStream();
        Assert.True(parameter.Write(ms, null!));
        Assert.Equal(1, ms.Length);

        ms.Position = 0;
        var parameter2 = new AVChannelParameter();
        Assert.True(parameter2.Read(ms, null!));
        Assert.Equal(0, parameter2.Count);
        Assert.Empty(parameter2.Items);
    }
}
