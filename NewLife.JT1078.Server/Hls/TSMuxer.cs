using System.Buffers;

namespace NewLife.JT1078.Server.Hls;

/// <summary>MPEG-TS 复用器。将 H.264 视频和 AAC 音频复用为 TS 段</summary>
/// <remarks>
/// 用于 HLS 直播分段，每个段包含：
/// 1. PAT + PMT（段起始）
/// 2. 多个视频 PES 包（H.264 NAL 单元）
/// 3. 多个音频 PES 包（AAC 帧）
/// </remarks>
internal class TSMuxer : IDisposable
{
    #region 属性
    /// <summary>是否有音频流</summary>
    public Boolean HasAudio { get; set; }

    /// <summary>当前写入缓冲区</summary>
    private readonly List<Byte[]> _packets = new();

    /// <summary>PAT 和 PMT 已写入标记</summary>
    private Boolean _psilWritten;

    // 连续性计数器
    private Byte _patCc;
    private Byte _pmtCc;
    private Byte _videoCc;
    private Byte _audioCc;

    private Boolean _disposed;
    #endregion

    #region 方法
    /// <summary>重置状态，开始新段</summary>
    public void Reset()
    {
        _packets.Clear();
        _psilWritten = false;
        _patCc = 0;
        _pmtCc = 0;
        _videoCc = 0;
        _audioCc = 0;
    }

    /// <summary>确保 PAT 和 PMT 已写入</summary>
    private void EnsurePsi()
    {
        if (_psilWritten) return;
        _psilWritten = true;

        // PAT
        var pat = WritePatPacket();
        _packets.Add(pat);

        // PMT
        var pmt = WritePmtPacket();
        _packets.Add(pmt);
    }

    /// <summary>写入一个 PAT 包</summary>
    private Byte[] WritePatPacket()
    {
        var data = new Byte[TSPacket.Length];
        var offset = 0;

        // TS Header (no adaptation)
        data[0] = TSPacket.SyncByte;
        data[1] = 0x40; // payload_unit_start_indicator = 1
        data[2] = 0x00; // PID = 0x0000 (PAT)
        data[3] = (Byte)(0x10 | _patCc);
        _patCc = (Byte)((_patCc + 1) & 0x0F);
        offset = 4;

        // pointer_field
        data[offset++] = 0x00;

        // PAT Section
        // table_id
        data[offset++] = 0x00;
        // section_syntax_indicator=1, reserved=0, reserved=3, section_length
        data[offset++] = 0xB0;
        data[offset++] = 0x0D;
        // transport_stream_id
        data[offset++] = 0x00;
        data[offset++] = 0x01;
        // reserved=3, version_number=0, current_next_indicator=1
        data[offset++] = 0xC1;
        // section_number, last_section_number
        data[offset++] = 0x00;
        data[offset++] = 0x00;
        // program_number = 0x0001
        data[offset++] = 0x00;
        data[offset++] = 0x01;
        // reserved=7, program_map_PID = 0x1000
        data[offset++] = 0xE0;
        data[offset++] = 0x00;

        // CRC32
        var crc = Crc32Mpeg2.Compute(data.AsSpan(4, offset - 4));
        data[offset++] = (Byte)((crc >> 24) & 0xFF);
        data[offset++] = (Byte)((crc >> 16) & 0xFF);
        data[offset++] = (Byte)((crc >> 8) & 0xFF);
        data[offset++] = (Byte)(crc & 0xFF);

        // Fill remaining bytes with padding
        while (offset < TSPacket.Length)
        {
            data[offset++] = 0xFF;
        }

        return data;
    }

    /// <summary>写入一个 PMT 包</summary>
    private Byte[] WritePmtPacket()
    {
        var data = new Byte[TSPacket.Length];
        var offset = 0;

        // TS Header (no adaptation)
        data[0] = TSPacket.SyncByte;
        data[1] = (Byte)(0x40 | (TSPacket.PidPmt >> 8)); // payload_unit_start_indicator = 1
        data[2] = (Byte)(TSPacket.PidPmt & 0xFF); // PID = 0x1000
        data[3] = (Byte)(0x10 | _pmtCc);
        _pmtCc = (Byte)((_pmtCc + 1) & 0x0F);
        offset = 4;

        // pointer_field
        data[offset++] = 0x00;

        // PMT Section
        // table_id = 0x02
        data[offset++] = 0x02;
        // section_syntax_indicator=1, reserved=0, reserved=3
        var sectionLengthPos = offset;
        data[offset++] = 0xB0;
        data[offset++] = 0x00; // placeholder
        // program_number
        data[offset++] = 0x00;
        data[offset++] = 0x01;
        // reserved=3, version_number=0, current_next_indicator=1
        data[offset++] = 0xC1;
        // section_number, last_section_number
        data[offset++] = 0x00;
        data[offset++] = 0x00;
        // reserved=7, PCR_PID = 0x1001 (video)
        data[offset++] = 0xE0;
        data[offset++] = 0x01;
        // reserved=4, program_info_length = 0
        data[offset++] = 0xF0;
        data[offset++] = 0x00;

        // Video stream (H.264)
        data[offset++] = TSPacket.StreamTypeH264; // stream_type
        data[offset++] = 0xE0; // reserved=7, elementary_PID high
        data[offset++] = 0x01; // elementary_PID low = 0x1001
        data[offset++] = 0xF0; // reserved=4, ES_info_length high
        data[offset++] = 0x00; // ES_info_length low = 0

        if (HasAudio)
        {
            // Audio stream (AAC)
            data[offset++] = TSPacket.StreamTypeAac;
            data[offset++] = 0xE0; // reserved=7, elementary_PID high
            data[offset++] = 0x02; // elementary_PID low = 0x1002
            data[offset++] = 0xF0;
            data[offset++] = 0x00;
        }

        // Update section_length
        var sectionLength = offset - 7 - 3; // from after section_length to end (CRC)
        data[sectionLengthPos] = (Byte)(0xB0 | ((sectionLength >> 8) & 0x0F));
        data[sectionLengthPos + 1] = (Byte)(sectionLength & 0xFF);

        // CRC32
        var crc = Crc32Mpeg2.Compute(data.AsSpan(4, offset - 4));
        data[offset++] = (Byte)((crc >> 24) & 0xFF);
        data[offset++] = (Byte)((crc >> 16) & 0xFF);
        data[offset++] = (Byte)((crc >> 8) & 0xFF);
        data[offset++] = (Byte)(crc & 0xFF);

        // Fill remaining
        while (offset < TSPacket.Length)
        {
            data[offset++] = 0xFF;
        }

        return data;
    }

    /// <summary>写入视频帧（H.264 NAL 单元）</summary>
    /// <param name="nalData">NAL 单元数据</param>
    /// <param name="pts">PTS 时间戳（90kHz 时钟）</param>
    public void WriteVideo(ReadOnlySpan<Byte> nalData, Int64 pts)
    {
        EnsurePsi();

        // Pack NAL data into PES packets, each spanning multiple TS packets
        var pesPackets = PacketizePes(nalData, TSPacket.StreamIdVideo, pts, ref _videoCc);
        foreach (var packet in pesPackets)
        {
            _packets.Add(packet);
        }
    }

    /// <summary>写入音频帧（AAC 数据）</summary>
    /// <param name="aacData">AAC 帧数据</param>
    /// <param name="pts">PTS 时间戳（90kHz 时钟）</param>
    public void WriteAudio(ReadOnlySpan<Byte> aacData, Int64 pts)
    {
        if (!HasAudio) return;
        EnsurePsi();

        var pesPackets = PacketizePes(aacData, TSPacket.StreamIdAudio, pts, ref _audioCc);
        foreach (var packet in pesPackets)
        {
            _packets.Add(packet);
        }
    }

    /// <summary>将 ES 数据打包为 PES 包的 TS 流</summary>
    private static List<Byte[]> PacketizePes(ReadOnlySpan<Byte> esData, Byte streamId, Int64 pts, ref Byte cc)
    {
        var packets = new List<Byte[]>();
        var pid = streamId == TSPacket.StreamIdVideo ? TSPacket.PidVideo : TSPacket.PidAudio;
        var dataOffset = 0;
        var remaining = esData.Length;
        var isFirst = true;

        while (remaining > 0)
        {
            var packet = new Byte[TSPacket.Length];
            var offset = 0;

            // TS Header
            packet[0] = TSPacket.SyncByte;
            if (isFirst)
            {
                packet[1] = (Byte)(0x40 | (pid >> 8)); // payload_unit_start_indicator
                packet[1] |= 0x10; // no adaptation field
            }
            else
            {
                packet[1] = (Byte)(pid >> 8);
                packet[1] |= 0x10; // no adaptation field
            }
            packet[2] = (Byte)(pid & 0xFF);
            packet[3] = (Byte)(0x10 | (cc & 0x0F));
            cc = (Byte)((cc + 1) & 0x0F);
            offset = 4;

            if (isFirst)
            {
                // PES Header
                packet[offset++] = 0x00;
                packet[offset++] = 0x00;
                packet[offset++] = 0x01;
                packet[offset++] = streamId;
                // PES_packet_length = 0 (unbounded)
                packet[offset++] = 0x00;
                packet[offset++] = 0x00;

                // PES header flags
                packet[offset++] = 0x80; // '10' + flags
                packet[offset++] = 0x80; // PTS_DTS_flags = 10 (PTS only)
                packet[offset++] = 0x05; // PES_header_data_length = 5

                // PTS (5 bytes, 90kHz)
                var ptsVal = (UInt64)pts;
                packet[offset++] = (Byte)(0x31 | ((ptsVal >> 30) & 0x0E));
                packet[offset++] = (Byte)((ptsVal >> 22) & 0xFF);
                packet[offset++] = (Byte)(0x01 | ((ptsVal >> 15) & 0x7E));
                packet[offset++] = (Byte)((ptsVal >> 7) & 0xFF);
                packet[offset++] = (Byte)(0x01 | ((ptsVal << 1) & 0x7E));

                isFirst = false;
            }
            else
            {
                // For continuation packets, no PES header - just data
                // But we need a minimal adaptation field to avoid stuffing bytes
                // Actually for continuation we just put data directly
            }

            // Fill packet with ES data
            var headerSize = offset;
            var avail = TSPacket.Length - headerSize;
            var copyLen = Math.Min(avail, remaining);

            if (copyLen > 0)
            {
                esData.Slice(dataOffset, copyLen).CopyTo(packet.AsSpan(offset));
                offset += copyLen;
                dataOffset += copyLen;
                remaining -= copyLen;
            }

            // Fill remaining bytes with padding (0xFF)
            while (offset < TSPacket.Length)
            {
                packet[offset++] = 0xFF;
            }

            packets.Add(packet);
        }

        return packets;
    }

    /// <summary>完成当前段，返回完整的 TS 段数据</summary>
    public Byte[] FinalizeSegment()
    {
        if (_packets.Count == 0) return [];

        // 计算总大小
        var totalSize = _packets.Sum(p => p.Length);
        var result = new Byte[totalSize];
        var offset = 0;

        foreach (var packet in _packets)
        {
            packet.CopyTo(result, offset);
            offset += packet.Length;
        }

        return result;
    }

    /// <summary>释放资源</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _packets.Clear();
    }
    #endregion
}
