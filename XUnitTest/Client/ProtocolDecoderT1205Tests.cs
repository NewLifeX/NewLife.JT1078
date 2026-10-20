using NewLife.JT1078.Client.Services;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Client;

public class ProtocolDecoderT1205Tests
{
    private static Byte[] BuildPayload(Boolean truncated = false)
    {
        var model = new T1205
        {
            SerialNumber = 7,
            Items =
            [
                new T1205ResourceInfo
                {
                    Channel = 1,
                    StartTime = new DateTime(2026, 10, 20, 10, 0, 0),
                    EndTime = new DateTime(2026, 10, 20, 10, 5, 0),
                    AlarmFlags = 3,
                    Kind = T1205ResourceKind.AudioVideo,
                    CodeStream = CodeStreams.Main,
                    StorageKind = T1205StorageKind.Main,
                    FileSize = 4096,
                },
                new T1205ResourceInfo
                {
                    Channel = 2,
                    StartTime = new DateTime(2026, 10, 20, 12, 0, 0),
                    EndTime = new DateTime(2026, 10, 20, 12, 30, 0),
                    AlarmFlags = 0,
                    Kind = T1205ResourceKind.Video,
                    CodeStream = CodeStreams.Sub,
                    StorageKind = T1205StorageKind.Backup,
                    FileSize = 8192,
                },
            ],
        };

        var ms = new MemoryStream();
        model.Write(ms, null!);
        var body = ms.ToArray();

        var payload = new Byte[body.Length + 2];
        payload[0] = 0x12;
        payload[1] = 0x05;

        if (truncated)
        {
            // 声明 2 条资源却只带 1 条完整数据
            Array.Copy(body, 0, payload, 2, 6 + 28);
            return payload[..(2 + 6 + 28)];
        }

        Array.Copy(body, 0, payload, 2, body.Length);
        return payload;
    }

    [Fact(DisplayName = "ProtocolDecoder 解码 T1205 资源列表")]
    public void Decode_T1205()
    {
        var decoded = ProtocolDecoder.Decode(BuildPayload());

        Assert.Equal(MessageKinds.AVResourceList, decoded.Kind);
        Assert.True(decoded.IsParsed);
        Assert.Contains(decoded.Fields, f => f.Name == "SerialNumber" && f.Value == "7");
        Assert.Contains(decoded.Fields, f => f.Name == "Count" && f.Value == "2");

        var first = decoded.Fields.FirstOrDefault(f => f.Name == "资源[0]");
        Assert.NotNull(first);
        Assert.Contains("通道 1", first.Value);
        Assert.Contains("2026-10-20 10:00:00", first.Value);
        Assert.Contains("AudioVideo", first.Value);
        Assert.Contains("4096 字节", first.Value);

        var second = decoded.Fields.FirstOrDefault(f => f.Name == "资源[1]");
        Assert.NotNull(second);
        Assert.Contains("通道 2", second.Value);
        Assert.Contains("Backup", second.Value);
    }

    [Fact(DisplayName = "ProtocolDecoder 提示 T1205 资源项截断")]
    public void Decode_T1205_Truncated()
    {
        var decoded = ProtocolDecoder.Decode(BuildPayload(truncated: true));

        Assert.True(decoded.IsParsed);
        Assert.Contains(decoded.Fields, f => f.Name == "⚠️ 资源项截断");
        Assert.Single(decoded.Fields, f => f.Name.StartsWith("资源["));
    }

    [Fact(DisplayName = "ProtocolDecoder 空载荷返回未知指令")]
    public void Decode_EmptyPayload()
    {
        var decoded = ProtocolDecoder.Decode([]);

        Assert.Equal("未知指令", decoded.KindName);
        Assert.Empty(decoded.Fields);
    }
}
