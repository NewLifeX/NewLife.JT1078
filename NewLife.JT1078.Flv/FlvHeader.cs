using System.Buffers.Binary;

namespace NewLife.JT1078.Flv;

/// <summary>FLV头部</summary>
/// <remarks>
/// Signature(3 Byte)+Version(1 Byte)+Flags(1 Bypte)+DataOffset(4 Byte)
/// Signature 占3个字节：
/// 固定FLV三个字符作为标示。一般发现前三个字符为FLV时就认为他是flv文件。
/// Version 占1个字节：
/// 标示FLV的版本号。这里我们看到是1
/// Flags 占1个字节：
/// 内容标示。第0位和第2位,分别表示 video 与 audio 存在的情况.(1表示存在,0表示不存在)。看到0x05，也就是00000101，代表既有视频，也有音频。
/// DataOffset 占4个字节：
/// 表示FLV的header长度。
/// </remarks>
public readonly struct FlvHeader
{
    public FlvHeader(Boolean hasVideo, Boolean hasAudio)
    {
        Byte audioFlag = 0x01 << 2;
        Byte videoFlag = 0x01;
        Flags = 0x00;
        if (hasAudio) Flags |= audioFlag;
        if (hasVideo) Flags |= videoFlag;
    }

    private readonly Byte Flags;

    public ReadOnlySpan<Byte> ToArray()
    {
        Span<Byte> tmp = new Byte[9];
        tmp[0] = 0x46;
        tmp[1] = 0x4c;
        tmp[2] = 0x56;
        tmp[3] = 0x01;
        tmp[4] = Flags;
        BinaryPrimitives.WriteInt32BigEndian(tmp[5..], 9);
        return tmp;
    }
}
