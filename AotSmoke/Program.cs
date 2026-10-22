using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using NewLife;
using NewLife.Data;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;

// NativeAOT 冒烟：协议编解码 + 消息体反射序列化 + 服务端装配 + 运行时多语言，确认裁剪器保留类型与词表
// 全球化不变模式（DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1）下无法构造 zh-Hans，保留默认文化并回退英文源文断言
var localized = false;
try
{
    Localization.Culture = new CultureInfo("zh-Hans");
    localized = true;
}
catch (CultureNotFoundException)
{
}

var msg = new JTMessage
{
    Version = 129,
    Type = AVTypes.H264,
    Completed = true,
    Sequence = 1,
    SIM = "138001380000",
    Channel = 1,
    DataType = DataTypes.VideoIFrame,
    PacketFlag = PacketFlags.Alone,
    Timestamp = 1,
    Payload = new ArrayPacket(new Byte[] { 1, 2, 3 }),
};

var codec = new JTCodec();
var pk = msg.ToPacket();
var rs = codec.TryParse(new System.Buffers.ReadOnlySequence<Byte>(pk.GetSpan().ToArray()));
if (rs == null || rs.Value.Invalid || rs.Value.BodyLength != 3) throw new InvalidOperationException("Frame codec failed.");

// 核心库词表：中文译名与英文源文不同；不变全球化模式下回退英文源文
var coreKey = "The message must be JTMessage.";
var text = Localization.Translate(coreKey);
if (localized)
{
    if (text == coreKey || !text.Contains("消息必须")) throw new InvalidOperationException("Core localization failed: " + text);
}
else if (text != coreKey) throw new InvalidOperationException("Core fallback failed: " + text);

// 消息体模型：自定义 Read/Write + Binary 反射序列化在裁剪后仍可用
var parameter = new AVChannelParameter
{
    Items = [new AVChannelParameterItem { ChennalId = 1, Pasrameter = new AVParameter() }],
};
var ms = new MemoryStream();
if (!parameter.Write(ms, null!)) throw new InvalidOperationException("Model write failed.");
ms.Position = 0;
var parameter2 = new AVChannelParameter();
if (!parameter2.Read(ms, null!) || parameter2.Count != 1 || parameter2.Items[0].ChennalId != 1)
    throw new InvalidOperationException("Model read failed.");

// 服务端装配（不启动监听）：协议编解码器已挂载
using var server = new JT1078Server { Port = 0 };
if (server.Protocol is not JTCodec) throw new InvalidOperationException("Server protocol not configured.");

// 服务端词表：模块初始化器在程序集加载后注册，日志模板按语言翻译或回退英文
var logKey = "JT1078 server started on port {0}";
var logText = Localization.Translate(logKey, 1078);
if (localized)
{
    if (!logText.Contains("服务端已启动")) throw new InvalidOperationException("Server localization failed: " + logText);
}
else if (logText != "JT1078 server started on port 1078") throw new InvalidOperationException("Server fallback failed: " + logText);

// DI 装配在裁剪后仍可解析服务
var services = new ServiceCollection();
services.AddJT1078Gateway();
using var provider = services.BuildServiceProvider();
var diServer = provider.GetRequiredService<JT1078Server>();
if (diServer.Protocol is not JTCodec) throw new InvalidOperationException("DI server protocol not configured.");

Console.WriteLine($"AOT OK: body={rs.Value.BodyLength}, message={text}, log={logText}, model={parameter2.Items.Length}");

return 0;
