namespace NewLife.JT1078.Server.Hls;

/// <summary>MPEG-TS 数据包常量与辅助方法</summary>
internal static class TSPacket
{
    /// <summary>TS 包固定长度</summary>
    public const Int32 Length = 188;

    /// <summary>同步字节</summary>
    public const Byte SyncByte = 0x47;

    // PID 定义
    /// <summary>PAT PID</summary>
    public const UInt16 PidPat = 0x0000;
    /// <summary>PMT PID</summary>
    public const UInt16 PidPmt = 0x1000;
    /// <summary>视频流 PID</summary>
    public const UInt16 PidVideo = 0x1001;
    /// <summary>音频流 PID</summary>
    public const UInt16 PidAudio = 0x1002;

    // StreamType 定义（H.264 / AAC）
    /// <summary>H.264 视频流类型</summary>
    public const Byte StreamTypeH264 = 0x1B;
    /// <summary>AAC 音频流类型</summary>
    public const Byte StreamTypeAac = 0x0F;

    // StreamId 定义
    /// <summary>视频 PES StreamId</summary>
    public const Byte StreamIdVideo = 0xE0;
    /// <summary>音频 PES StreamId</summary>
    public const Byte StreamIdAudio = 0xC0;

    /// <summary>写入 TS 包头</summary>
    public static void WriteHeader(Span<Byte> buffer, UInt16 pid, Boolean payloadStart, ref Byte continuityCounter)
    {
        buffer[0] = SyncByte;
        buffer[1] = (Byte)(0x10 | (payloadStart ? 0x40 : 0x00) | ((pid >> 8) & 0x1F));
        buffer[2] = (Byte)(pid & 0xFF);
        buffer[3] = (Byte)(0x10 | (continuityCounter & 0x0F)); // payload_only + continuity_counter

        continuityCounter = (Byte)((continuityCounter + 1) & 0x0F);
    }

    /// <summary>写入自适应字段（带 PCR）</summary>
    public static Int32 WriteAdaptationField(Span<Byte> buffer, Int32 offset, Int32 pcr)
    {
        // adaptation_field_length (8) + PCR (6) = 7 bytes
        buffer[offset] = 6; // adaptation_field_length
        buffer[offset + 1] = 0x10; // PCR_flag = 1

        // PCR: 33 bits base (90kHz) + 6 bits extension (27MHz)
        // 6 bytes: base(33) + reserved(6) + extension(9)
        var pcrBase = (UInt64)pcr;
        buffer[offset + 2] = (Byte)((pcrBase >> 25) & 0xFF);
        buffer[offset + 3] = (Byte)((pcrBase >> 17) & 0xFF);
        buffer[offset + 4] = (Byte)((pcrBase >> 9) & 0xFF);
        buffer[offset + 5] = (Byte)((pcrBase >> 1) & 0xFF);
        buffer[offset + 6] = (Byte)(((pcrBase & 0x01) << 7) | 0x7E); // extension 6 bits = 0
        buffer[offset + 7] = 0x00;

        return 8; // 1(length) + 7(data)
    }

    /// <summary>写入 PAT 段数据到指定缓冲区，返回写入字节数</summary>
    public static Int32 WritePat(Span<Byte> buffer, ref Byte cc)
    {
        var offset = 0;

        // TS Header
        WriteHeader(buffer, PidPat, true, ref cc);
        offset = 4;

        // pointer_field
        buffer[offset++] = 0x00;

        // PSI Section: PAT
        // table_id (8)
        buffer[offset++] = 0x00; // PAT
        // section_syntax_indicator(1)=1, reserved(1)=0, reserved(2)=3, section_length(12)=13
        buffer[offset++] = 0xB0;
        buffer[offset++] = 0x0D;
        // transport_stream_id (16)
        buffer[offset++] = 0x00;
        buffer[offset++] = 0x01;
        // reserved(2)=3, version_number(5)=0, current_next_indicator(1)=1
        buffer[offset++] = 0xC1;
        // section_number (8)
        buffer[offset++] = 0x00;
        // last_section_number (8)
        buffer[offset++] = 0x00;

        // program_number (16) = 0x0001
        buffer[offset++] = 0x00;
        buffer[offset++] = 0x01;
        // reserved(3)=7, program_map_PID(13)=0x1000
        buffer[offset++] = 0xE0;
        buffer[offset++] = 0x00;

        // CRC32 (after B2 from section_length+3)
        // section data starts at offset 3 (after pointer_field), length = 0x0D
        var crcOffset = offset - 3;
        var crc = Crc32Mpeg2.Compute(buffer.Slice(3, offset - 3));
        buffer[offset++] = (Byte)((crc >> 24) & 0xFF);
        buffer[offset++] = (Byte)((crc >> 16) & 0xFF);
        buffer[offset++] = (Byte)((crc >> 8) & 0xFF);
        buffer[offset++] = (Byte)(crc & 0xFF);

        return offset;
    }

    /// <summary>写入 PMT 段数据到指定缓冲区，返回写入字节数</summary>
    public static Int32 WritePmt(Span<Byte> buffer, Boolean hasAudio, ref Byte cc)
    {
        var offset = 0;

        // TS Header
        WriteHeader(buffer, PidPmt, true, ref cc);
        offset = 4;

        // pointer_field
        buffer[offset++] = 0x00;

        // PSI Section: PMT
        // table_id (8)
        buffer[offset++] = 0x02; // PMT
        // section_syntax_indicator(1)=1, reserved(1)=0, reserved(2)=3
        var sectionLengthPos = offset;
        buffer[offset++] = 0xB0;
        buffer[offset++] = 0x00; // placeholder
        // program_number (16)
        buffer[offset++] = 0x00;
        buffer[offset++] = 0x01;
        // reserved(2)=3, version_number(5)=0, current_next_indicator(1)=1
        buffer[offset++] = 0xC1;
        // section_number (8)
        buffer[offset++] = 0x00;
        // last_section_number (8)
        buffer[offset++] = 0x00;
        // reserved(3)=7, PCR_PID(13)=0x1001 (video pid)
        buffer[offset++] = 0xE0;
        buffer[offset++] = 0x01;
        // reserved(4)=0xF, program_info_length(12)=0
        buffer[offset++] = 0xF0;
        buffer[offset++] = 0x00;

        // Video Stream: H.264
        // stream_type (8)
        buffer[offset++] = StreamTypeH264;
        // reserved(3)=7, elementary_PID(13)=0x1001
        buffer[offset++] = 0xE0;
        buffer[offset++] = 0x01;
        // reserved(4)=0xF, ES_info_length(12)=0
        buffer[offset++] = 0xF0;
        buffer[offset++] = 0x00;

        if (hasAudio)
        {
            // Audio Stream: AAC
            buffer[offset++] = StreamTypeAac;
            buffer[offset++] = 0xE0;
            buffer[offset++] = 0x02;
            buffer[offset++] = 0xF0;
            buffer[offset++] = 0x00;
        }

        // Calculate section_length and update placeholder
        var sectionLength = offset - 7 - 3; // from after section_length to end (including CRC)
        buffer[sectionLengthPos] = (Byte)(0xB0 | ((sectionLength >> 8) & 0x0F));
        buffer[sectionLengthPos + 1] = (Byte)(sectionLength & 0xFF);

        // CRC32
        var crc = Crc32Mpeg2.Compute(buffer.Slice(3, offset - 3));
        buffer[offset++] = (Byte)((crc >> 24) & 0xFF);
        buffer[offset++] = (Byte)((crc >> 16) & 0xFF);
        buffer[offset++] = (Byte)((crc >> 8) & 0xFF);
        buffer[offset++] = (Byte)(crc & 0xFF);

        return offset;
    }

    /// <summary>写入 PES 包（用于视频或音频）</summary>
    public static Int32 WritePes(Span<Byte> buffer, Byte streamId, Int64 pts, ReadOnlySpan<Byte> data, ref Byte cc)
    {
        var offset = 0;

        // TS Header
        WriteHeader(buffer, streamId == StreamIdVideo ? PidVideo : PidAudio, true, ref cc);
        offset = 4;

        // 计算 PES 包数据
        var pesPayloadOffset = 0;
        var remaining = data.Length;
        var firstPacket = true;

        while (remaining > 0)
        {
            if (firstPacket)
            {
                // PES header
                // start_code_prefix (24)
                buffer[offset++] = 0x00;
                buffer[offset++] = 0x00;
                buffer[offset++] = 0x01;
                // stream_id (8)
                buffer[offset++] = streamId;
                // PES_packet_length (16) - 0 = unbounded
                buffer[offset++] = 0x00;
                buffer[offset++] = 0x00;

                // PES header flags
                buffer[offset++] = 0x80; // '10' + scrambling=0 + priority=0 + alignment=0 + copyright=0 + original=0
                buffer[offset++] = 0x80; // PTS_DTS_flags=10 (PTS only)
                // PES_header_data_length
                buffer[offset++] = 0x05; // 5 bytes for PTS

                // PTS (5 bytes, 90kHz)
                // PTS: 33 bits
                // 4 bits '0011' + 3 bits PTS[32:30] + 1 bit '1'
                // 15 bits PTS[29:15] + 1 bit '1'
                // 15 bits PTS[14:0] + 1 bit '1'
                var ptsVal = (UInt64)pts;
                buffer[offset++] = (Byte)(0x31 | ((ptsVal >> 30) & 0x0E));
                buffer[offset++] = (Byte)((ptsVal >> 22) & 0xFF);
                buffer[offset++] = (Byte)(0x01 | ((ptsVal >> 15) & 0x7E));
                buffer[offset++] = (Byte)((ptsVal >> 7) & 0xFF);
                buffer[offset++] = (Byte)(0x01 | ((ptsVal << 1) & 0x7E));

                firstPacket = false;
            }
            else
            {
                // Subsequent TS packets for the same PES - just continue
                WriteHeader(buffer.Slice(offset - 4), streamId == StreamIdVideo ? PidVideo : PidAudio, false, ref cc);
                // offset already past header from previous iteration
            }

            // Calculate available payload space
            var headerSize = offset % Length;
            var avail = Length - headerSize;

            var copyLen = Math.Min(avail, remaining);
            if (copyLen > 0)
            {
                data.Slice(pesPayloadOffset, copyLen).CopyTo(buffer.Slice(offset));
                offset += copyLen;
                pesPayloadOffset += copyLen;
                remaining -= copyLen;
            }

            // If we filled the TS packet and there's more data, start a new TS packet
            if (remaining > 0 && offset % Length == 0)
            {
                // Add TS header for next packet
                var pid = streamId == StreamIdVideo ? PidVideo : PidAudio;
                WriteHeader(buffer.Slice(offset, 4), pid, false, ref cc);
                offset += 4;
            }
        }

        return offset;
    }
}

/// <summary>CRC32 MPEG-2 计算辅助类</summary>
internal static class Crc32Mpeg2
{
    private static readonly UInt32[] Table = GenerateTable();

    private static UInt32[] GenerateTable()
    {
        var table = new UInt32[256];
        for (var i = 0; i < 256; i++)
        {
            var crc = (UInt32)i << 24;
            for (var j = 0; j < 8; j++)
            {
                crc = (crc & 0x80000000) != 0 ? (crc << 1) ^ 0x04C11DB7 : crc << 1;
            }
            table[i] = crc;
        }
        return table;
    }

    /// <summary>计算 CRC32 MPEG-2 校验值</summary>
    public static UInt32 Compute(ReadOnlySpan<Byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
        {
            crc = (crc << 8) ^ Table[((crc >> 24) ^ b) & 0xFF];
        }
        return crc;
    }
}
