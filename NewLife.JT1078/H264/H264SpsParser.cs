using NewLife.Video.Codecs;

namespace NewLife.JT1078.H264;

/// <summary>H.264 SPS 安全解析器</summary>
/// <remarks>
/// NewLife.Video 的 <see cref="ExpGolombReader"/> 在数据不足时会无限递归导致栈溢出，
/// 而 SPS 来自网络设备，可能存在截断或异常数据。本解析器使用带边界检查的位读取，
/// 解析失败时返回 false，避免异常数据导致服务进程崩溃。
/// </remarks>
public static class H264SpsParser
{
    /// <summary>尝试解析 SPS，获取分辨率与档次信息</summary>
    /// <param name="nal">SPS NAL 单元，包含起始码之后的 NAL 头字节，且已剔除防竞争字节（0x03）</param>
    /// <param name="info">解析结果，失败时为默认值</param>
    /// <returns>解析成功返回 true</returns>
    public static Boolean TryParse(ReadOnlySpan<Byte> nal, out SPSInfo info)
    {
        info = new SPSInfo();

        // NAL 头 + profile_idc + constraint_flags + level_idc
        if (nal.Length < 4) return false;

        var reader = new SpsBitReader(nal[1..]);
        var profileIdc = (Byte)reader.ReadBits(8);
        var profileCompat = reader.ReadBits(5);
        reader.SkipBits(3);
        var levelIdc = (Byte)reader.ReadBits(8);
        if (!reader.IsValid) return false;

        if (!reader.SkipUe()) return false; // seq_parameter_set_id

        switch (profileIdc)
        {
            case 100:
            case 110:
            case 122:
            case 244:
            case 44:
            case 83:
            case 86:
            case 118:
            case 128:
                if (!SkipHighProfile(ref reader)) return false;
                break;
        }

        if (!reader.SkipUe()) return false; // log2_max_frame_num_minus4

        var pocType = reader.ReadUe();
        if (!reader.IsValid) return false;
        if (pocType == 0)
        {
            if (!reader.SkipUe()) return false; // log2_max_pic_order_cnt_lsb_minus4
        }
        else if (pocType == 1)
        {
            reader.SkipBits(1);  // delta_pic_order_always_zero_flag
            if (!reader.SkipSe()) return false; // offset_for_non_ref_pic
            if (!reader.SkipSe()) return false; // offset_for_top_to_bottom_field

            var numRefFrames = reader.ReadUe();
            if (!reader.IsValid || numRefFrames > 255) return false;
            for (var i = 0; i < numRefFrames; i++)
            {
                if (!reader.SkipSe()) return false; // offset_for_ref_frame
            }
        }
        else if (pocType > 2)
            return false;

        if (!reader.SkipUe()) return false; // max_num_ref_frames
        reader.SkipBits(1);                 // gaps_in_frame_num_value_allowed_flag

        var widthInMbs = reader.ReadUe();
        var heightInMapUnits = reader.ReadUe();
        var frameMbsOnly = reader.ReadBits(1);
        if (!reader.IsValid) return false;
        if (frameMbsOnly == 0) reader.SkipBits(1); // mb_adaptive_frame_field_flag
        reader.SkipBits(1);                        // direct_8x8_inference_flag

        UInt32 cropLeft = 0, cropRight = 0, cropTop = 0, cropBottom = 0;
        if (reader.ReadBits(1) == 1)
        {
            cropLeft = reader.ReadUe();
            cropRight = reader.ReadUe();
            cropTop = reader.ReadUe();
            cropBottom = reader.ReadUe();
            if (!reader.IsValid) return false;
        }

        var width = (Int32)((widthInMbs + 1) * 16 - cropLeft * 2 - cropRight * 2);
        var height = (Int32)((2 - frameMbsOnly) * (heightInMapUnits + 1) * 16 - (frameMbsOnly == 1 ? 2 : 4) * (cropTop + cropBottom));
        if (width <= 0 || height <= 0) return false;

        info = new SPSInfo
        {
            profileIdc = profileIdc,
            profileCompat = profileCompat,
            levelIdc = levelIdc,
            width = width,
            height = height,
        };
        return true;
    }

    private static Boolean SkipHighProfile(ref SpsBitReader reader)
    {
        var chromaFormatIdc = reader.ReadUe();
        if (!reader.IsValid || chromaFormatIdc > 3) return false;
        if (chromaFormatIdc == 3) reader.SkipBits(1); // separate_colour_plane_flag

        if (!reader.SkipUe()) return false; // bit_depth_luma_minus8
        if (!reader.SkipUe()) return false; // bit_depth_chroma_minus8
        reader.SkipBits(1);                 // qpprime_y_zero_transform_bypass_flag

        if (reader.ReadBits(1) == 1)        // seq_scaling_matrix_present_flag
        {
            var count = chromaFormatIdc != 3 ? 8 : 12;
            for (var i = 0; i < count; i++)
            {
                if (reader.ReadBits(1) == 1) // seq_scaling_list_present_flag
                {
                    if (!SkipScalingList(ref reader, i < 6 ? 16 : 64)) return false;
                }
            }
        }

        return reader.IsValid;
    }

    private static Boolean SkipScalingList(ref SpsBitReader reader, Int32 size)
    {
        Int32 lastScale = 8, nextScale = 8;
        for (var j = 0; j < size; j++)
        {
            if (nextScale != 0)
            {
                var deltaScale = reader.ReadSe();
                if (!reader.IsValid) return false;
                nextScale = (lastScale + deltaScale + 256) % 256;
            }
            lastScale = nextScale == 0 ? lastScale : nextScale;
        }

        return true;
    }

    /// <summary>带边界检查的位读取器</summary>
    private ref struct SpsBitReader
    {
        private readonly ReadOnlySpan<Byte> _data;
        private Int32 _bitOffset;

        public SpsBitReader(ReadOnlySpan<Byte> data)
        {
            _data = data;
            _bitOffset = 0;
            IsValid = true;
        }

        public Boolean IsValid { get; private set; }

        private readonly Int32 Remaining => _data.Length * 8 - _bitOffset;

        public UInt32 ReadBits(Int32 count)
        {
            if (count < 0 || count > 32 || Remaining < count)
            {
                IsValid = false;
                return 0;
            }

            var value = 0U;
            for (var i = 0; i < count; i++)
            {
                var index = _bitOffset++;
                value = (value << 1) | (UInt32)((_data[index >> 3] >> (7 - (index & 7))) & 1);
            }

            return value;
        }

        public void SkipBits(Int32 count)
        {
            if (count < 0 || Remaining < count)
            {
                IsValid = false;
                return;
            }

            _bitOffset += count;
        }

        public UInt32 ReadUe()
        {
            var leadingZeros = 0;
            while (true)
            {
                if (Remaining < 1)
                {
                    IsValid = false;
                    return 0;
                }
                if (ReadBits(1) == 1) break;
                leadingZeros++;
                if (leadingZeros > 31)
                {
                    IsValid = false;
                    return 0;
                }
            }

            var suffix = ReadBits(leadingZeros);
            if (!IsValid) return 0;
            return (UInt32)((1L << leadingZeros) - 1 + suffix);
        }

        public Boolean SkipUe()
        {
            ReadUe();
            return IsValid;
        }

        public Int32 ReadSe()
        {
            var value = ReadUe();
            if (!IsValid) return 0;

            return (value & 1) == 1 ? (Int32)((value + 1) >> 1) : -(Int32)(value >> 1);
        }

        public Boolean SkipSe()
        {
            ReadSe();
            return IsValid;
        }
    }
}
