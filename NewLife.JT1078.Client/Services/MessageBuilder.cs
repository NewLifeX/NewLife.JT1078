using NewLife.Data;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.Services;

/// <summary>JTMessage 构建辅助</summary>
public static class MessageBuilder
{
    private static Int32 _sequence;

    /// <summary>构建一个完整的 JTMessage</summary>
    public static JTMessage Build(
        AVTypes avType,
        DataTypes dataType,
        PacketFlags packetFlag,
        String sim,
        Byte channel,
        Boolean completed = true,
        UInt64? timestamp = null,
        UInt16? lastIFrameInterval = null,
        UInt16? lastFrameInterval = null,
        Byte[]? payload = null)
    {
        var seq = (UInt16)Interlocked.Increment(ref _sequence);

        var msg = new JTMessage
        {
            Version = 129,
            Type = avType,
            Completed = completed,
            Sequence = seq,
            SIM = sim,
            Channel = channel,
            DataType = dataType,
            PacketFlag = packetFlag,
        };

        // 时间戳：非透传数据时有时间戳
        if (dataType != DataTypes.Transparent)
        {
            msg.Timestamp = timestamp ?? (UInt64)(DateTime.Now.Ticks / 10000);
        }

        // 视频帧有帧间隔字段
        if (dataType == DataTypes.VideoIFrame)
        {
            msg.LastIFrameInterval = lastIFrameInterval ?? 0;
            msg.LastFrameInterval = lastFrameInterval ?? 0;
        }
        else if (dataType == DataTypes.VideoPFrame || dataType == DataTypes.VideoBFrame)
        {
            msg.LastFrameInterval = lastFrameInterval ?? 33;
        }

        if (payload != null && payload.Length > 0)
        {
            msg.Payload = new ArrayPacket(payload);
        }

        return msg;
    }

    /// <summary>构建 H264 视频 I 帧消息</summary>
    public static JTMessage BuildVideoIFrame(String sim, Byte channel, Byte[]? naluData = null)
    {
        return Build(
            AVTypes.H264,
            DataTypes.VideoIFrame,
            PacketFlags.Alone,
            sim, channel,
            payload: naluData ?? GenerateTestNalu()
        );
    }

    /// <summary>构建 G711A 音频帧消息</summary>
    public static JTMessage BuildAudioFrame(String sim, Byte channel, Byte[]? audioData = null)
    {
        return Build(
            AVTypes.G711A,
            DataTypes.AudioFrame,
            PacketFlags.Alone,
            sim, channel,
            payload: audioData ?? [0xD5, 0x55, 0xD5, 0x55]
        );
    }

    /// <summary>生成测试用 H.264 NAL 单元数据</summary>
    public static Byte[] GenerateTestNalu()
    {
        // SPS + PPS + IDR 切片
        return
        [
            0x00, 0x00, 0x01, 0x67, 0x42, 0x00, 0x1E, 0x8D,
            0x00, 0x00, 0x01, 0x68, 0xEB, 0xE3, 0xCB, 0x22, 0xC0,
        ];
    }
}
