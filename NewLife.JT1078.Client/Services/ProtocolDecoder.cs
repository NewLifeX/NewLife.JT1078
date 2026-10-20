using System.Reflection;
using NewLife.JT1078.Client.Models;
using NewLife.JT1078.Models;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.Services;

/// <summary>协议指令解码器。解析 JT1078 信令载荷为可读字段</summary>
public static class ProtocolDecoder
{
    private static readonly Dictionary<MessageKinds, Type> _kindMap;
    private static readonly Dictionary<MessageKinds, String> _descriptions;

    static ProtocolDecoder()
    {
        _kindMap = [];
        _descriptions = [];

        // 扫描所有标注了 [MessageKind] 的类型
        foreach (var type in typeof(MessageKinds).Assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<MessageKindAttribute>();
            if (attr != null)
            {
                _kindMap[attr.Kind] = type;
            }
        }

        // 消息说明
        _descriptions[MessageKinds.QueryAVAttribute] = "平台查询终端音视频属性";
        _descriptions[MessageKinds.UploadAVAttribute] = "终端上传音视频属性";
        _descriptions[MessageKinds.UploadPassengerFlow] = "终端上传乘客流量";
        _descriptions[MessageKinds.AVTransmitRequest] = "平台请求实时音视频传输";
        _descriptions[MessageKinds.AVTransmitControl] = "平台控制实时音视频传输";
        _descriptions[MessageKinds.AVStatusNotify] = "终端通知实时音视频传输状态";
        _descriptions[MessageKinds.QueryResourceList] = "平台查询音视频资源列表";
        _descriptions[MessageKinds.FileUploadCommand] = "平台下发文件上传指令";
        _descriptions[MessageKinds.FileUploadControl] = "平台控制文件上传";
        _descriptions[MessageKinds.AVResourceList] = "终端上传音视频资源列表";
        _descriptions[MessageKinds.FileUploadComplete] = "终端通知文件上传完成";
        _descriptions[MessageKinds.PlaybackRequest] = "平台请求远程录像回放";
        _descriptions[MessageKinds.PlaybackControl] = "平台控制远程录像回放";
    }

    /// <summary>根据 MessageKinds 获取对应的模型类型</summary>
    public static Type? GetType(MessageKinds kind) => _kindMap.GetValueOrDefault(kind);

    /// <summary>获取消息说明</summary>
    public static String GetDescription(MessageKinds kind) => _descriptions.GetValueOrDefault(kind, "");

    /// <summary>列出所有已知的消息类型</summary>
    public static MessageKinds[] GetKnownKinds() => [.. _kindMap.Keys];

    /// <summary>解码消息载荷</summary>
    /// <param name="payload">完整载荷字节（含 MessageKind 头）</param>
    /// <returns>解码后的消息，若未知类型则 Fields 为空</returns>
    public static DecodedMessage Decode(Byte[] payload)
    {
        if (payload == null || payload.Length < 2)
            return UnknownMessage(payload);

        // 读取 MessageKind（大端 UInt16）
        var kind = (MessageKinds)(payload[0] << 8 | payload[1]);
        var body = payload.Length > 2 ? payload[2..] : [];

        var result = new DecodedMessage
        {
            Kind = kind,
            KindName = kind.ToString(),
            Description = GetDescription(kind),
            RawHex = BitConverter.ToString(payload),
        };

        try
        {
            DecodeBody(kind, body, result);
        }
        catch (Exception ex)
        {
            result.Fields.Add(new DecodedField { Name = "⚠️ 解码错误", Value = ex.Message });
        }

        return result;
    }

    /// <summary>生成未知消息的返回</summary>
    private static DecodedMessage UnknownMessage(Byte[]? payload)
    {
        var result = new DecodedMessage
        {
            KindName = "未知指令",
            Description = "无法识别的消息类型",
            RawHex = payload != null ? BitConverter.ToString(payload) : "",
        };
        if (payload != null && payload.Length >= 2)
        {
            var kind = (UInt16)(payload[0] << 8 | payload[1]);
            result.Fields.Add(new DecodedField { Name = "MessageKind", Value = $"0x{kind:X4}" });
        }
        return result;
    }

    /// <summary>根据消息类型解码消息体</summary>
    private static void DecodeBody(MessageKinds kind, Byte[] body, DecodedMessage result)
    {
        var st = 0; // 当前读取偏移
        result.IsParsed = false;

        switch (kind)
        {
            case MessageKinds.QueryAVAttribute:
                // 空体消息
                result.IsParsed = true;
                break;

            case MessageKinds.UploadAVAttribute:
                result.IsParsed = true;
                // AudioCodec(1) + AudioChannels(1) + AudioRate(1) + AudioBit(1) + AudioFrameLength(2) + AudioOutput(1) + VideoCodec(1) + MaxAudioChannels(1) + MaxVideoChannels(1)
                result.Fields.Add(ReadByte(body, ref st, "AudioCodec", "音频编码方式"));
                result.Fields.Add(ReadByte(body, ref st, "AudioChannels", "音频声道数"));
                result.Fields.Add(ReadByte(body, ref st, "AudioRate", "音频采样率"));
                result.Fields.Add(ReadByte(body, ref st, "AudioBit", "音频采样位数"));
                result.Fields.Add(ReadUInt16(body, ref st, "AudioFrameLength", "音频帧长度"));
                result.Fields.Add(ReadByte(body, ref st, "AudioOutput", "是否支持音频输出"));
                result.Fields.Add(ReadByte(body, ref st, "VideoCodec", "视频编码方式"));
                result.Fields.Add(ReadByte(body, ref st, "MaxAudioChannels", "最大音频物理通道数"));
                result.Fields.Add(ReadByte(body, ref st, "MaxVideoChannels", "最大视频物理通道数"));
                break;

            case MessageKinds.AVTransmitRequest:
                result.IsParsed = true;
                // ServerIP(4) + TcpPort(2) + UdpPort(2) + Channel(1) + MediaKind(1) + CodeStream(1)
                result.Fields.Add(ReadIP(body, ref st, "Server", "服务器IP"));
                result.Fields.Add(ReadUInt16(body, ref st, "TcpPort", "TCP端口"));
                result.Fields.Add(ReadUInt16(body, ref st, "UdpPort", "UDP端口"));
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadByte(body, ref st, "MediaKind", "数据类型"));
                result.Fields.Add(ReadByte(body, ref st, "CodeStream", "码流类型"));
                break;

            case MessageKinds.AVTransmitControl:
                result.IsParsed = true;
                // Channel(1) + Command(1) + CloseKind(1) + CodeStream(1)
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadByte(body, ref st, "Command", "控制指令"));
                result.Fields.Add(ReadByte(body, ref st, "CloseKind", "关闭音视频类型"));
                result.Fields.Add(ReadByte(body, ref st, "CodeStream", "切换码流类型"));
                break;

            case MessageKinds.AVStatusNotify:
                result.IsParsed = true;
                // Channel(1) + LossRate(1) + ErrorRate(1)
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadByte(body, ref st, "LossRate", "丢包率（%）"));
                result.Fields.Add(ReadByte(body, ref st, "ErrorRate", "误码率（%）"));
                break;

            case MessageKinds.PlaybackRequest:
                result.IsParsed = true;
                // ServerIP(4) + TcpPort(2) + UdpPort(2) + Channel(1) + FileKind(1) + CodeStream(1) + StorageKind(1) + Mode(1) + Multiple(1) + StartTime(6) + EndTime(6)
                result.Fields.Add(ReadIP(body, ref st, "Server", "服务器IP"));
                result.Fields.Add(ReadUInt16(body, ref st, "TcpPort", "TCP端口"));
                result.Fields.Add(ReadUInt16(body, ref st, "UdpPort", "UDP端口"));
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadByte(body, ref st, "FileKind", "音视频资源类型"));
                result.Fields.Add(ReadByte(body, ref st, "CodeStream", "码流类型"));
                result.Fields.Add(ReadByte(body, ref st, "StorageKind", "存储器类型"));
                result.Fields.Add(ReadByte(body, ref st, "Mode", "回放方式"));
                result.Fields.Add(ReadByte(body, ref st, "Multiple", "快进/快退倍数"));
                result.Fields.Add(ReadBCDTime(body, ref st, "StartTime", "开始时间"));
                result.Fields.Add(ReadBCDTime(body, ref st, "EndTime", "结束时间"));
                break;

            case MessageKinds.PlaybackControl:
                result.IsParsed = true;
                // Channel(1) + Control(1) + Multiple(1) + NewTime(6)
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadByte(body, ref st, "Control", "回放控制"));
                result.Fields.Add(ReadByte(body, ref st, "Multiple", "快进/快退倍数"));
                result.Fields.Add(ReadBCDTime(body, ref st, "NewTime", "拖动回放位置"));
                break;

            case MessageKinds.QueryResourceList:
                result.IsParsed = true;
                // Channel(1) + StartTime(6) + EndTime(6) + AlarmFlags(8) + FileKind(1) + CodeStream(1) + StorageKind(1)
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadBCDTime(body, ref st, "StartTime", "开始时间"));
                result.Fields.Add(ReadBCDTime(body, ref st, "EndTime", "结束时间"));
                result.Fields.Add(ReadUInt64(body, ref st, "AlarmFlags", "报警标志"));
                result.Fields.Add(ReadByte(body, ref st, "FileKind", "音视频资源类型"));
                result.Fields.Add(ReadByte(body, ref st, "CodeStream", "码流类型"));
                result.Fields.Add(ReadByte(body, ref st, "StorageKind", "存储器类型"));
                break;

            case MessageKinds.FileUploadCommand:
                result.IsParsed = true;
                // Server(32) + Port(2) + UserName(49) + Password(33) + Path(200) + Channel(1) + StartTime(6) + EndTime(6) + AlarmFlags(8) + FileKind(1)
                result.Fields.Add(ReadFixedString(body, ref st, 32, "Server", "FTP服务器IP"));
                result.Fields.Add(ReadUInt16(body, ref st, "Port", "FTP端口"));
                result.Fields.Add(ReadFixedString(body, ref st, 49, "UserName", "FTP用户名"));
                result.Fields.Add(ReadFixedString(body, ref st, 33, "Password", "FTP密码"));
                result.Fields.Add(ReadFixedString(body, ref st, 200, "Path", "文件上传路径"));
                result.Fields.Add(ReadByte(body, ref st, "Channel", "逻辑通道号"));
                result.Fields.Add(ReadBCDTime(body, ref st, "StartTime", "开始时间"));
                result.Fields.Add(ReadBCDTime(body, ref st, "EndTime", "结束时间"));
                result.Fields.Add(ReadUInt64(body, ref st, "AlarmFlags", "报警标志"));
                result.Fields.Add(ReadByte(body, ref st, "FileKind", "音视频资源类型"));
                break;

            case MessageKinds.FileUploadControl:
                result.IsParsed = true;
                // SerialNumber(2) + Control(1)
                result.Fields.Add(ReadInt16(body, ref st, "SerialNumber", "流水号"));
                result.Fields.Add(ReadByte(body, ref st, "Control", "控制指令"));
                break;

            case MessageKinds.FileUploadComplete:
                result.IsParsed = true;
                // SerialNumber(2) + Result(1)
                result.Fields.Add(ReadInt16(body, ref st, "SerialNumber", "流水号"));
                result.Fields.Add(ReadByte(body, ref st, "Result", "结果"));
                break;

            case MessageKinds.UploadPassengerFlow:
                result.IsParsed = true;
                // StartTime(6) + EndTime(6) + GetOnCount(2) + GetOffCount(2)
                result.Fields.Add(ReadBCDTime(body, ref st, "StartTime", "起始时间"));
                result.Fields.Add(ReadBCDTime(body, ref st, "EndTime", "结束时间"));
                result.Fields.Add(ReadUInt16(body, ref st, "GetOnCount", "上车人数"));
                result.Fields.Add(ReadUInt16(body, ref st, "GetOffCount", "下车人数"));
                break;

            case MessageKinds.AVResourceList:
                result.IsParsed = true;
                // SerialNumber(2) + Count(4) + Count × Item(28)
                result.Fields.Add(ReadInt16(body, ref st, "SerialNumber", "流水号"));

                var countField = ReadInt32(body, ref st, "Count", "资源总数");
                result.Fields.Add(countField);
                var count = Int32.TryParse(countField.Value, out var c) ? c : 0;

                for (var i = 0; i < count; i++)
                {
                    // 单条资源项固定 28 字节，数据不足时停止并提示
                    if (body.Length - st < 28)
                    {
                        result.Fields.Add(new DecodedField { Name = "⚠️ 资源项截断", Value = $"声明 {count} 项，实际解析 {i} 项" });
                        break;
                    }

                    var start = st;
                    var channel = body[st++];
                    var startTime = ReadBCDTime(body, ref st, "StartTime");
                    var endTime = ReadBCDTime(body, ref st, "EndTime");
                    var alarms = ReadUInt64(body, ref st, "AlarmFlags");
                    var resourceKind = (T1205ResourceKind)body[st++];
                    var codeStream = (CodeStreams)body[st++];
                    var storage = (T1205StorageKind)body[st++];
                    var size = ReadInt32(body, ref st, "FileSize");

                    result.Fields.Add(new DecodedField
                    {
                        Name = $"资源[{i}]",
                        Value = $"通道 {channel}｜{startTime.Value} ~ {endTime.Value}｜{resourceKind}｜码流 {codeStream}｜{storage}｜{size.Value} 字节｜报警 {alarms.Value}",
                        RawHex = BitConverter.ToString(body.AsSpan(start, 28).ToArray()),
                        Description = "音视频资源项",
                    });
                }
                break;

            // 其余复杂消息暂不做完整解析
            default:
                result.Fields.Add(new DecodedField { Name = "⚠️ 未实现解码", Value = $"消息类型 {kind} (0x{(UInt16)kind:X4}) 的解码器尚未实现" });
                break;
        }

        // 显示剩余未解析字节
        if (st < body.Length)
        {
            var remain = body[st..];
            result.Fields.Add(new DecodedField
            {
                Name = "剩余数据",
                Value = $"{remain.Length} 字节",
                RawHex = BitConverter.ToString(remain),
            });
        }
    }

    #region 读取辅助方法
    private static DecodedField ReadByte(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, 1).ToArray();
        var value = data[offset];
        offset += 1;
        return new DecodedField { Name = name, Value = $"0x{value:X2} ({value})", RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadUInt16(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, 2).ToArray();
        var value = (UInt16)(data[offset] << 8 | data[offset + 1]);
        offset += 2;
        return new DecodedField { Name = name, Value = value.ToString(), RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadInt16(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, 2).ToArray();
        var value = (Int16)(data[offset] << 8 | data[offset + 1]);
        offset += 2;
        return new DecodedField { Name = name, Value = value.ToString(), RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadUInt64(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, 8).ToArray();
        var value = 0UL;
        for (var i = 0; i < 8; i++)
            value = (value << 8) | data[offset + i];
        offset += 8;
        return new DecodedField { Name = name, Value = $"0x{value:X16}", RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadInt32(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, 4).ToArray();
        var value = (Int32)((UInt32)data[offset] << 24 | (UInt32)data[offset + 1] << 16 | (UInt32)data[offset + 2] << 8 | data[offset + 3]);
        offset += 4;
        return new DecodedField { Name = name, Value = value.ToString(), RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadIP(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, 4).ToArray();
        var value = $"{data[offset]}.{data[offset + 1]}.{data[offset + 2]}.{data[offset + 3]}";
        offset += 4;
        return new DecodedField { Name = name, Value = value, RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadFixedString(Byte[] data, ref Int32 offset, Int32 length, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, Math.Min(length, data.Length - offset)).ToArray();
        // 读取直到第一个 0x00 或达到 length
        var end = Array.IndexOf(raw, (Byte)0);
        var value = end >= 0 ? System.Text.Encoding.UTF8.GetString(raw, 0, end) : System.Text.Encoding.UTF8.GetString(raw);
        offset += length;
        return new DecodedField { Name = name, Value = value, RawHex = BitConverter.ToString(raw), Description = desc };
    }

    private static DecodedField ReadBCDTime(Byte[] data, ref Int32 offset, String name, String? desc = null)
    {
        var raw = data.AsSpan(offset, Math.Min(6, data.Length - offset)).ToArray();
        // BCD 编码：每字节两位 BCD，如 0x24 0x07 0x14 0x15 0x30 0x00 → "240714153000"
        var sb = new System.Text.StringBuilder();
        foreach (var b in raw)
        {
            sb.Append((b >> 4).ToString("X1"));
            sb.Append((b & 0x0F).ToString("X1"));
        }
        var bcdStr = sb.ToString();
        var value = bcdStr.Length == 12
            ? $"20{bcdStr[..2]}-{bcdStr[2..4]}-{bcdStr[4..6]} {bcdStr[6..8]}:{bcdStr[8..10]}:{bcdStr[10..12]}"
            : bcdStr;
        offset += raw.Length;
        return new DecodedField { Name = name, Value = value, RawHex = BitConverter.ToString(raw), Description = desc };
    }
    #endregion
}
