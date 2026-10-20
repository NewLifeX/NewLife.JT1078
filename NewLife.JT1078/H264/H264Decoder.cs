using NewLife.Data;
using NewLife.JT1078.Protocols;
using NewLife.Video.Codecs;

namespace NewLife.JT1078.H264
{
    public class H264Decoder
    {
        /// <summary>获取连续负载，分段链式数据先合并为连续缓冲区</summary>
        /// <param name="packet">负载</param>
        /// <param name="buffer">分段时返回合并后的缓冲区，供 Span 引用保持存活</param>
        /// <returns>连续数据的 Span</returns>
        private static ReadOnlySpan<Byte> GetSpan(IPacket packet, out Byte[]? buffer)
        {
            buffer = null;
            if (packet.Next == null) return packet.GetSpan();

            buffer = packet.ToArray();
            return buffer.AsSpan();
        }

        /// <summary>
        /// <see cref="https://github.com/samirkumardas/jmuxer/blob/master/src/parsers/h264.js"/>
        /// </summary>
        /// <param name="package"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<H264NALU> ParseNALU(JTMessage package, String key = null)
        {
            var h264NALUs = new List<H264NALU>();
            Int32 i = 0, state = 0, laststate = 0;
            Int32? lastIndex = null;
            Int32 length = package.Payload.Total;
            Byte value;
            ReadOnlySpan<Byte> buffer = GetSpan(package.Payload, out _);
            while (i < length)
            {
                value = buffer[i++];
                switch (state)
                {
                    case 0:
                        if (value == 0) state = 1;
                        break;
                    case 1:
                        state = value == 0 ? 2 : 0;
                        break;
                    case 2:
                    case 3:
                        if (value == 0)
                            state = 3;
                        else if (value == 1 && i < length)
                        {
                            if (lastIndex.HasValue)
                            {
                                var tmp = buffer.Slice(lastIndex.Value, i - state - 1 - lastIndex.Value);
                                h264NALUs.Add(Create(package, tmp, state + 1));
                            }
                            lastIndex = i;
                            laststate = state + 1;
                            state = 0;
                        }
                        else
                            state = 0;
                        break;
                    default:
                        break;
                }
            }
            if (lastIndex.HasValue)
                h264NALUs.Add(Create(package, buffer.Slice(lastIndex.Value), laststate));
            return h264NALUs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="package"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public AVFrame ParseAVFrame(JTMessage package, String key = null)
        {
            var jT1078AVFrame = new AVFrame
            {
                LogicChannelNumber = package.Channel,
                SIM = package.SIM,
                Nalus = new List<H264NALU>()
            };
            Int32 i = 0, state = 0, laststate = 0;
            Int32? lastIndex = null;
            Int32 length = package.Payload.Total;
            Byte value;
            ReadOnlySpan<Byte> buffer = GetSpan(package.Payload, out _);
            while (i < length)
            {
                value = buffer[i++];
                switch (state)
                {
                    case 0:
                        if (value == 0) state = 1;
                        break;
                    case 1:
                        state = value == 0 ? 2 : 0;
                        break;
                    case 2:
                    case 3:
                        if (value == 0)
                            state = 3;
                        else if (value == 1 && i < length)
                        {
                            if (lastIndex.HasValue)
                            {
                                var tmp = buffer.Slice(lastIndex.Value, i - state - 1 - lastIndex.Value);
                                var nalu = Create(package, tmp, state + 1);
                                if (nalu.NALUHeader.NalUnitType == NalUnitType.PPS)
                                    jT1078AVFrame.PPS = nalu;
                                else if (nalu.NALUHeader.NalUnitType == NalUnitType.SPS)
                                {
                                    jT1078AVFrame.SPS = nalu;
                                    var rawData = DiscardEmulationPreventionBytes(jT1078AVFrame.SPS.RawData);
                                    if (H264SpsParser.TryParse(rawData, out var spsInfo))
                                    {
                                        jT1078AVFrame.Width = spsInfo.width;
                                        jT1078AVFrame.Height = spsInfo.height;
                                        jT1078AVFrame.LevelIdc = spsInfo.levelIdc;
                                        jT1078AVFrame.ProfileIdc = spsInfo.profileIdc;
                                        jT1078AVFrame.ProfileCompat = (Byte)spsInfo.profileCompat;
                                    }
                                }
                                jT1078AVFrame.Nalus.Add(nalu);
                            }
                            lastIndex = i;
                            laststate = state + 1;
                            state = 0;
                        }
                        else
                            state = 0;
                        break;
                    default:
                        break;
                }
            }
            if (lastIndex.HasValue)
            {
                var nalu = Create(package, buffer.Slice(lastIndex.Value), laststate);
                if (nalu.NALUHeader.NalUnitType == NalUnitType.PPS)
                    jT1078AVFrame.PPS = nalu;
                else if (nalu.NALUHeader.NalUnitType == NalUnitType.SPS)
                {
                    jT1078AVFrame.SPS = nalu;
                    var rawData = DiscardEmulationPreventionBytes(jT1078AVFrame.SPS.RawData);
                    if (H264SpsParser.TryParse(rawData, out var spsInfo))
                    {
                        jT1078AVFrame.Width = spsInfo.width;
                        jT1078AVFrame.Height = spsInfo.height;
                        jT1078AVFrame.LevelIdc = spsInfo.levelIdc;
                        jT1078AVFrame.ProfileIdc = spsInfo.profileIdc;
                        jT1078AVFrame.ProfileCompat = (Byte)spsInfo.profileCompat;
                    }
                }
                jT1078AVFrame.Nalus.Add(nalu);
            }
            return jT1078AVFrame;
        }

        /// <summary>
        /// 
        /// <see cref="https://github.com/samirkumardas/jmuxer/blob/master/src/parsers/h264.js"/>
        /// </summary>
        /// <param name="package"></param>
        /// <param name="h264NALUs"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public void ParseNALU(JTMessage package, List<H264NALU> h264NALUs, String key = null)
        {
            Int32 i = 0, state = 0, laststate = 0;
            Int32? lastIndex = null;
            Int32 length = package.Payload.Total;
            Byte value;
            ReadOnlySpan<Byte> buffer = GetSpan(package.Payload, out _);
            while (i < length)
            {
                value = buffer[i++];
                switch (state)
                {
                    case 0:
                        if (value == 0) state = 1;
                        break;
                    case 1:
                        state = value == 0 ? 2 : 0;
                        break;
                    case 2:
                    case 3:
                        if (value == 0)
                            state = 3;
                        else if (value == 1 && i < length)
                        {
                            if (lastIndex.HasValue)
                            {
                                var tmp = buffer.Slice(lastIndex.Value, i - state - 1 - lastIndex.Value);
                                h264NALUs.Add(Create(package, tmp, lastIndex.Value));
                            }
                            lastIndex = i;
                            laststate = state + 1;
                            state = 0;
                        }
                        else
                            state = 0;
                        break;
                    default:
                        break;
                }
            }
            if (lastIndex.HasValue)
                h264NALUs.Add(Create(package, buffer.Slice(lastIndex.Value), laststate));
        }

        private H264NALU Create(JTMessage package, ReadOnlySpan<Byte> nalu, Int32 startCodePrefix)
        {
            var nALU = new H264NALU
            {
                SIM = package.SIM,
                DataType = package.DataType,
                Channel = package.Channel,
                LastFrameInterval = package.LastFrameInterval,
                LastIFrameInterval = package.LastIFrameInterval,
                Timestamp = package.Timestamp,
                RawData = nalu.ToArray()
            };
            if (startCodePrefix == 3)
                nALU.StartCodePrefix = H264NALU.Start1;
            else if (startCodePrefix == 4)
                nALU.StartCodePrefix = H264NALU.Start2;
            nALU.NALUHeader = new NALUHeader(nalu.Slice(0, 1));
            return nALU;
        }

        /// <summary>
        /// Expunge any "Emulation Prevention" bytes from a "Raw Byte Sequence Payload"
        /// <see href="https://blog.csdn.net/u011399342/article/details/80472084"/>
        /// 剔除用于防止竞争字节误判的0x03填充
        /// </summary>
        /// <param name="srcBuffer"></param>
        /// <returns></returns>
        public Byte[] DiscardEmulationPreventionBytes(ReadOnlySpan<Byte> srcBuffer)
        {
            var length = srcBuffer.Length;
            var EPBPositions = new List<Int32>();
            var i = 1;
            // Find all `Emulation Prevention Bytes`
            while (i < length - 2)
                if (srcBuffer[i] == 0 && srcBuffer[i + 1] == 0 && srcBuffer[i + 2] == 0x03)
                {
                    EPBPositions.Add(i + 2);
                    i += 2;
                }
                else
                    i++;
            // If no Emulation Prevention Bytes were found just return the original
            // array
            if (EPBPositions.Count == 0)
                return srcBuffer.ToArray();
            // Create a new array to hold the NAL unit data
            var newLength = length - EPBPositions.Count;
            var newBuffer = new Byte[newLength];
            var sourceIndex = 0;
            for (i = 0; i < newLength; sourceIndex++, i++)
            {
                if (EPBPositions.Any() && sourceIndex == EPBPositions[0])
                {
                    // Skip this byte
                    sourceIndex++;
                    // Remove this position index
                    EPBPositions.RemoveAt(0);
                }
                newBuffer[i] = srcBuffer[sourceIndex];
            }
            return newBuffer;
        }
    }
}
