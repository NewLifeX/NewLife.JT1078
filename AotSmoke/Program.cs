using System.Globalization;
using NewLife;
using NewLife.Data;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;
using NewLife.JT1078.Server;

// NativeAOT 冒烟：协议编解码 + 消息体反射序列化 + 服务端装配 + 运行时多语言，确认裁剪器保留类型与词表
Localization.Culture = new CultureInfo("zh-Hans");

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

var text = Localization.Translate("The message must be JTMessage.");
if (!text.Contains("JTMessage")) throw new InvalidOperationException("Localization failed.");

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

Console.WriteLine($"AOT OK: body={rs.Value.BodyLength}, message={text}, model={parameter2.Items.Length}");

return 0;
