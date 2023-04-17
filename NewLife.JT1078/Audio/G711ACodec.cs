namespace NewLife.JT1078.Audio
{
    public class G711ACodec : IAudioCodec
    {
        private readonly Int32 SIGN_BIT = 0x80;
        private readonly Int32 QUANT_MASK = 0xf;
        private readonly Int32 SEG_SHIFT = 4;
        private readonly Int32 SEG_MASK = 0x70;

        private readonly Int16[] seg_end = { 0xFF, 0x1FF, 0x3FF, 0x7FF, 0xFFF, 0x1FFF, 0x3FFF, 0x7FFF };
        private Int16 AlawToLinear(Byte value)
        {
            Int16 t;
            Int16 seg;
            value ^= 0x55;
            t = (Int16)((value & QUANT_MASK) << 4);
            seg = (Int16)((value & SEG_MASK) >> SEG_SHIFT);
            switch (seg)
            {
                case 0:
                    t += 8;
                    break;
                case 1:
                    t += 0x108;
                    break;
                default:
                    t += 0x108;
                    t <<= seg - 1;
                    break;
            }
            return (value & SIGN_BIT) != 0 ? t : (Int16)(-t);
        }

        public Byte[] ToPcm(ReadOnlySpan<Byte> audio, IAudioAttachData audioAttachData)
        {
            //// 如果前四字节是00 01 52 00，则是海思头，需要去掉
            //if (audio[0] == 0x00 && audio[1] == 0x01 && audio[2] == 0x52 && audio[3] == 0x00)
            //    audio = audio[4..];

            var pcmdata = new Byte[audio.Length * 2];
            for (Int32 i = 0, offset = 0; i < audio.Length; i++)
            {
                var value = AlawToLinear(audio[i]);
                pcmdata[offset++] = (Byte)(value & 0xff);
                pcmdata[offset++] = (Byte)(value >> 8 & 0xff);
            }
            return pcmdata;
        }

        private static Int16 Search(Int16 val, Int16[] table, Int16 size)
        {
            for (Int16 i = 0; i < size; i++)
            {
                if (val <= table[i])
                {
                    return i;
                }
            }
            return size;
        }

        private Byte LinearToAlaw(Int16 pcm_val)
        {
            Int16 mask;
            Int16 seg;
            Char aval;
            if (pcm_val >= 0)
            {
                mask = 0xD5;
            }
            else
            {
                mask = 0x55;
                pcm_val = (Int16)(-pcm_val - 1);
                if (pcm_val < 0)
                {
                    pcm_val = 32767;
                }
            }

            //Convert the scaled magnitude to segment number.
            seg = Search(pcm_val, seg_end, 8);

            //Combine the sign, segment, and quantization bits.
            if (seg >= 8)
            {
                //out of range, return maximum value.
                return (Byte)(0x7F ^ mask);
            }
            else
            {
                aval = (Char)(seg << SEG_SHIFT);
                if (seg < 2) aval |= (Char)((pcm_val >> 4) & QUANT_MASK);
                else aval |= (Char)((pcm_val >> (seg + 3)) & QUANT_MASK);
                return (Byte)(aval ^ mask);
            }
        }

        /// <summary>转至G711</summary>
        /// <param name="pcm"></param>
        /// <returns></returns>
        public Byte[] FromPcm(ReadOnlySpan<Byte> pcm, IAudioAttachData audioAttachData)
        {
            var g711data = new Byte[pcm.Length / 2];
            for (Int32 i = 0, k = 0; i < pcm.Length - 1; i += 2, k++)
            {
                var v = (Int16)((pcm[i + 1] << 8) | (pcm[i]));
                g711data[k] = LinearToAlaw(v);
            }
            return g711data;
        }
    }
}