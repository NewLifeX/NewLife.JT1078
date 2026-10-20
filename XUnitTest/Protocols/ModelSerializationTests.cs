using NewLife.JT1078.Models;
using NewLife.JT1078.Serialization;
using NewLife.Serialization;
using Xunit;

namespace XUnitTest.Protocols;

/// <summary>消息体模型序列化测试</summary>
public class ModelSerializationTests
{
    private class BcdModel
    {
        [BCDString(6)]
        public String SIM { get; set; }

        [BCDTime(6, "yyMMddHHmmss")]
        public DateTime Time { get; set; }
    }

    [Fact(DisplayName = "BCD 特性通过 Binary 序列化往返")]
    public void BcdAttributes_RoundTrip()
    {
        var model = new BcdModel
        {
            SIM = "138001380000",
            Time = new DateTime(2026, 10, 20, 11, 30, 45),
        };

        var ms = new MemoryStream();
        Binary.FastWrite(model, ms);
        Assert.Equal(12, ms.Length);

        ms.Position = 0;
        var model2 = Binary.FastRead<BcdModel>(ms);

        Assert.Equal(model.SIM, model2.SIM);
        Assert.Equal(model.Time, model2.Time);
    }

    [Fact(DisplayName = "T9201 BCD 时间字段序列化往返")]
    public void T9201_BcdTime_RoundTrip()
    {
        var model = new T9201
        {
            Server = "127.0.0.1",
            TcpPort = 1078,
            Channel = 1,
            StartTime = new DateTime(2026, 10, 20, 11, 0, 0),
            EndTime = new DateTime(2026, 10, 20, 12, 0, 0),
        };

        var ms = new MemoryStream();
        Binary.FastWrite(model, ms);

        ms.Position = 0;
        var model2 = Binary.FastRead<T9201>(ms);

        Assert.Equal(model.Server, model2.Server);
        Assert.Equal(model.TcpPort, model2.TcpPort);
        Assert.Equal(model.Channel, model2.Channel);
        Assert.Equal(model.StartTime, model2.StartTime);
        Assert.Equal(model.EndTime, model2.EndTime);
    }

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
