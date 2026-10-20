using NewLife.JT1078.Models;
using Xunit;

namespace XUnitTest.Protocols;

public class T1205Tests
{
    private static T1205 CreateModel() => new()
    {
        SerialNumber = 0x1234,
        Items =
        [
            new T1205ResourceInfo
            {
                Channel = 1,
                StartTime = new DateTime(2026, 10, 20, 10, 0, 0),
                EndTime = new DateTime(2026, 10, 20, 10, 5, 30),
                AlarmFlags = 5,
                Kind = T1205ResourceKind.AudioVideo,
                CodeStream = CodeStreams.Main,
                StorageKind = T1205StorageKind.Main,
                FileSize = 1024 * 1024,
            },
            new T1205ResourceInfo
            {
                Channel = 2,
                StartTime = new DateTime(2026, 10, 20, 11, 0, 0),
                EndTime = new DateTime(2026, 10, 20, 11, 30, 0),
                AlarmFlags = 0,
                Kind = T1205ResourceKind.Video,
                CodeStream = CodeStreams.Sub,
                StorageKind = T1205StorageKind.Backup,
                FileSize = 2048,
            },
        ],
    };

    [Fact(DisplayName = "T1205 资源列表往返序列化")]
    public void T1205_RoundTrip()
    {
        var model = CreateModel();

        var ms = new MemoryStream();
        Assert.True(model.Write(ms, null!));
        Assert.Equal(6 + 28 * 2, ms.Length);

        ms.Position = 0;
        var model2 = new T1205();
        Assert.True(model2.Read(ms, null!));

        Assert.Equal(0x1234, model2.SerialNumber);
        Assert.Equal(2, model2.Items.Count);

        var first = model2.Items[0];
        Assert.Equal((Byte)1, first.Channel);
        Assert.Equal(new DateTime(2026, 10, 20, 10, 0, 0), first.StartTime);
        Assert.Equal(new DateTime(2026, 10, 20, 10, 5, 30), first.EndTime);
        Assert.Equal(5UL, first.AlarmFlags);
        Assert.Equal(T1205ResourceKind.AudioVideo, first.Kind);
        Assert.Equal(CodeStreams.Main, first.CodeStream);
        Assert.Equal(T1205StorageKind.Main, first.StorageKind);
        Assert.Equal(1024 * 1024, first.FileSize);

        var second = model2.Items[1];
        Assert.Equal((Byte)2, second.Channel);
        Assert.Equal(T1205ResourceKind.Video, second.Kind);
        Assert.Equal(CodeStreams.Sub, second.CodeStream);
        Assert.Equal(T1205StorageKind.Backup, second.StorageKind);
        Assert.Equal(2048, second.FileSize);
    }

    [Fact(DisplayName = "T1205 空资源列表只写头部")]
    public void T1205_Empty()
    {
        var model = new T1205 { SerialNumber = 7 };

        var ms = new MemoryStream();
        Assert.True(model.Write(ms, null!));
        Assert.Equal(6, ms.Length);

        ms.Position = 0;
        var model2 = new T1205();
        Assert.True(model2.Read(ms, null!));
        Assert.Equal(7, model2.SerialNumber);
        Assert.Empty(model2.Items);
    }

    [Fact(DisplayName = "T1205 资源项截断时保留已解析部分")]
    public void T1205_TruncatedItem()
    {
        var ms = new MemoryStream();
        Assert.True(CreateModel().Write(ms, null!));
        var bytes = ms.ToArray();

        // 保留头部 + 第一条资源项 + 半条多余数据
        var truncated = bytes[..(6 + 28 + 10)];
        var model = new T1205();
        Assert.True(model.Read(new MemoryStream(truncated), null!));

        Assert.Single(model.Items);
        Assert.Equal((Byte)1, model.Items[0].Channel);
    }

    [Fact(DisplayName = "T1205 头部不足时解析失败")]
    public void T1205_ShortHeader()
    {
        var model = new T1205();

        Assert.False(model.Read(new MemoryStream(new Byte[4]), null!));
        Assert.Empty(model.Items);
    }
}
