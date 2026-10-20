using NewLife.Audio;
using NewLife.Audio.AAC;
using NewLife.Video.Codecs;
using NewLife.Video.Flv;
using NewLife.Video.Flv.Enums;
using NewLife.Video.Flv.MessagePack;
using NewLife.Video.Flv.Metadata;
using NewLife.JT1078.H264;
using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Flv;

/// <summary>Flv编码器。一个客户端对应一个实例</summary>
/// <remarks>
/// 一般一个flv文件由一个头部信息，一个script Tag，以及若干个video Tag和audio Tag组成。
/// <para>
/// 当实例不使用时，尽量手动调用下 Dispose
/// </para>
/// 
/// 手动编码
/// 0、EncoderFlvHeader
/// 1、插入  PriviousTagSize =0  always equal 0
/// 2、EncoderScriptTag
/// 3、EncoderFirstVideoTag
/// 4、EncoderFirstAudioTag
/// 5、EncoderVideoTag第二个参数传false
/// 6、EncoderAudioTag第二个参数传false
/// 自动编码
/// 1、EncoderFlvHeader
/// 2、EncoderScriptTag
/// 3、EncoderVideoTag第二个参数传true
/// 4、EncoderAudioTag第二个参数传true
/// </remarks>
public class FlvEncoder
{
    private readonly H264Decoder h264Decoder;
    private readonly AudioCodecFactory audioCodecFactory;
    private FaacEncoder _faacEncoder;

    //public FlvEncoder(int sampleRate = 8000, int channels = 1, int sampleBit = 16, bool adts = false)
    /// <summary>
    /// 
    /// </summary>
    public FlvEncoder(AudioCodecFactory audioCodec = null)
    {
        audioCodecFactory = audioCodec ?? new AudioCodecFactory();
        h264Decoder = new H264Decoder();
        //_faacEncoder = new FaacEncoder(8000, 1, 16, false);
    }

    /// <summary>
    /// 编码flv头
    /// </summary>
    /// <param name="hasVideo">是否有视频</param>
    /// <param name="hasAudio">是否有音频</param>
    /// <returns></returns>
    public Byte[] EncoderFlvHeader(Boolean hasVideo = true, Boolean hasAudio = false)
    {
        var flvHeader = new FlvHeader(hasVideo, hasAudio);
        return flvHeader.ToArray().ToArray();
    }

    /// <summary>
    /// 编码脚本Tag
    /// </summary>
    /// <param name="avframe">解析后的av信息</param>
    /// <param name="hasAudio">是否有音频</param>
    /// <param name="frameRate">帧率 默认25d 即每秒25帧</param>
    /// <returns></returns>
    public Byte[] EncoderScriptTag(AVFrame avframe, Boolean hasAudio = false, Double frameRate = 25d)
    {
        var buffer = FlvArrayPool.Rent(1024);
        try
        {
            var flvMessagePackWriter = new FlvMessagePackWriter(buffer);
            //flv body script tag
            //flv body tag header
            var flvTags = new FlvTags
            {
                Type = TagType.ScriptData,
                //flv body tag body
                DataTagsData = new Amf3
                {
                    Amf3Metadatas = new List<IAmf3Metadata>
                    {
                        new Amf3Metadata_Duration{Value = 0d},
                        new Amf3Metadata_VideoDataRate{Value = 0d},
                        new Amf3Metadata_VideoCodecId{Value = 7d},
                        new Amf3Metadata_FrameRate{Value = frameRate},
                        new Amf3Metadata_Width(){
                            Value=avframe.Width
                        },
                        new Amf3Metadata_Height(){
                            Value=avframe.Height
                        },
                    }
                }
            };
            if (hasAudio)
            {
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_AudioCodecId());
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_AudioSampleRate());
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_AudioSampleSize());
                flvTags.DataTagsData.Amf3Metadatas.Add(new Amf3Metadata_AudioStereo());
            }
            flvMessagePackWriter.WriteFlvTag(flvTags);
            flvMessagePackWriter.WriteUInt32((UInt32)(flvTags.DataSize + 11));
            return flvMessagePackWriter.FlushAndGetArray();
        }
        finally
        {
            FlvArrayPool.Return(buffer);
        }
    }

    /// <summary>
    ///  编码首帧视频，即videoTag[0]
    /// </summary>
    /// <param name="avframe"></param>
    /// <returns></returns>
    public Byte[] EncoderFirstVideoTag(AVFrame avframe)
    {
        var buffer = FlvArrayPool.Rent(4096);
        try
        {
            var flvMessagePackWriter = new FlvMessagePackWriter(buffer);
            //flv body video tag
            //flv body tag header
            var flvTags = new FlvTags
            {
                Type = TagType.Video,
                Timestamp = (UInt32)avframe.SPS.Timestamp,
                TimestampExt = 0,
                StreamId = 0,
                //flv body tag body
                VideoTagsData = new VideoTags()
            };
            flvTags.VideoTagsData.FrameType = FrameType.KeyFrame;
            flvTags.VideoTagsData.VideoData = new AvcVideoPacke
            {
                AvcPacketType = AvcPacketType.SequenceHeader,
                CompositionTime = 0
            };
            var aVCDecoderConfigurationRecord = new AVCDecoderConfigurationRecord
            {
                AVCProfileIndication = avframe.ProfileIdc,
                ProfileCompatibility = avframe.ProfileCompat,
                AVCLevelIndication = avframe.LevelIdc,
                NumOfPictureParameterSets = 1,
                PPSBuffer = avframe.PPS.RawData,
                SPSBuffer = avframe.SPS.RawData
            };
            flvTags.VideoTagsData.VideoData.AVCDecoderConfiguration = aVCDecoderConfigurationRecord;
            flvMessagePackWriter.WriteFlvTag(flvTags);
            flvMessagePackWriter.WriteUInt32((UInt32)(flvTags.DataSize + 11));
            return flvMessagePackWriter.FlushAndGetArray();
        }
        finally
        {
            FlvArrayPool.Return(buffer);
        }
    }

    /// <summary>
    /// 编码首帧音频，即audioTag[0]
    /// </summary>
    /// <param name="timestamp"></param>
    /// <returns></returns>
    public Byte[] EncoderFirstAudioTag(UInt64 timestamp)
    {
        var buffer = FlvArrayPool.Rent(2048);
        try
        {
            var flvMessagePackWriter = new FlvMessagePackWriter(buffer);
            //flv body audio tag
            //flv body tag header
            var flvTags = new FlvTags
            {
                Type = TagType.Audio,
                Timestamp = (UInt32)timestamp,
                //flv body tag body
                AudioTagsData = new AudioTags(AACPacketType.AudioSpecificConfig)
            };
            flvMessagePackWriter.WriteFlvTag(flvTags);
            flvMessagePackWriter.WriteUInt32((UInt32)(flvTags.DataSize + 11));
            return flvMessagePackWriter.FlushAndGetArray();
        }
        finally
        {
            FlvArrayPool.Return(buffer);
        }
    }

    /// <summary>
    /// 编码视频
    /// </summary>
    /// <remarks><paramref name="package"/>必须是组包后的数据</remarks>
    /// <param name="package">1078包</param>
    /// <param name="needVideoHeader">是否需要首帧视频</param>
    /// <returns></returns>
    public Byte[] EncoderVideoTag(JTMessage package, Boolean needVideoHeader = false, Boolean hasAudio = false)
    {
        if (package.DataType == DataTypes.AudioFrame) return default;

        var buffer = FlvArrayPool.Rent(package.Payload.Total * 2 + 4096);
        var flvMessagePackWriter = new FlvMessagePackWriter(buffer);
        var nalus = h264Decoder.ParseNALU(package);
        if (nalus != null && nalus.Count > 0)
        {
            var sei = nalus.FirstOrDefault(x => x.NALUHeader.NalUnitType == NalUnitType.SEI);
            var sps = nalus.FirstOrDefault(x => x.NALUHeader.NalUnitType == NalUnitType.SPS);
            var pps = nalus.FirstOrDefault(x => x.NALUHeader.NalUnitType == NalUnitType.PPS);
            nalus.Remove(sps);
            nalus.Remove(pps);
            nalus.Remove(sei);

            if (needVideoHeader)
            {
                //flv header
                var flvHeader = EncoderFlvHeader(true, hasAudio);
                flvMessagePackWriter.WriteArray(flvHeader);
                // always 0
                flvMessagePackWriter.WriteUInt32(0);
                //解析sps
                if (sps == null) return null;

                var rawData = h264Decoder.DiscardEmulationPreventionBytes(sps.RawData);
                var h264GolombReader = new ExpGolombReader(rawData);
                var spsInfo = h264GolombReader.ReadSPS();
                var frame = new AVFrame
                {
                    Width = spsInfo.width,
                    Height = spsInfo.height,
                    ProfileIdc = spsInfo.profileIdc,
                    ProfileCompat = (Byte)spsInfo.profileCompat,
                    LevelIdc = spsInfo.levelIdc,

                    SPS = sps,
                    PPS = pps,
                };

                //script tag
                var scriptTag = EncoderScriptTag(frame, hasAudio);
                flvMessagePackWriter.WriteArray(scriptTag);
                // first video tag
                var firstVideoTag = EncoderFirstVideoTag(frame);
                flvMessagePackWriter.WriteArray(firstVideoTag);
            }
            foreach (var naln in nalus)
            {
                var otherVideoTag = EncoderOtherVideoTag(naln);
                flvMessagePackWriter.WriteArray(otherVideoTag);
            }
        }
        return flvMessagePackWriter.FlushAndGetArray();
    }

    /// <summary>
    /// 编码非首帧音频
    /// </summary>
    /// <param name="package"></param>
    /// <param name="needAacHeader">是否需要首帧音频</param>
    /// <returns></returns>
    public Byte[] EncoderAudioTag(JTMessage package, Boolean needAacHeader = false)
    {
        if (package.DataType != DataTypes.AudioFrame) throw new Exception(L.T("Incorrect parameter, package must be audio frame"));

        var flvMessagePackWriter = new FlvMessagePackWriter(new Byte[65536]);
        if (needAacHeader)
        {
            flvMessagePackWriter.WriteArray(EncoderFirstAudioTag(package.Timestamp));
        }

        // 编码成功，此时为一帧aac音频数据
        var pcm = audioCodecFactory.ToPcm((NewLife.Audio.AVTypes)package.Type, package.Payload);
        if (pcm != null && pcm.Total > 0)
        {
            var pcmBytes = new Byte[pcm.Total];
            for (var i = 0; i < pcmBytes.Length; i++) pcmBytes[i] = pcm[i];
            var aacFrameData = new Byte[4096];
            var count = _faacEncoder.Encode(pcmBytes, aacFrameData);
            if (count > 0)
            {
                // Data Tag Frame
                flvMessagePackWriter.WriteArray(EncoderAacAudioTag((UInt32)package.Timestamp, aacFrameData.ReadBytes(0, count)));
            }
        }
        return flvMessagePackWriter.FlushAndGetArray();
    }

    /// <summary>
    /// 编码非首帧视频
    /// </summary>
    /// <param name="nALU"></param>
    /// <returns></returns>
    public Byte[] EncoderOtherVideoTag(H264NALU nALU)
    {
        var buffer = FlvArrayPool.Rent(nALU.RawData.Length * 2 + 4096);
        try
        {
            var flvMessagePackWriter = new FlvMessagePackWriter(buffer);
            //flv body video tag
            //flv body tag header
            var flvTags = new FlvTags
            {
                Type = TagType.Video,
                //pts
                Timestamp = (UInt32)nALU.Timestamp,
                TimestampExt = 0,
                StreamId = 0,
                //flv body tag body
                VideoTagsData = new VideoTags()
            };
            flvTags.VideoTagsData.VideoData = new AvcVideoPacke
            {
                AvcPacketType = AvcPacketType.Raw
            };
            //1: keyframe (for AVC, a seekable frame) —— 即H.264的IDR帧；
            //2: inter frame(for AVC, a non - seekable frame) —— H.264的普通I帧；
            //ref:https://www.cnblogs.com/chyingp/p/flv-getting-started.html
            if (nALU.NALUHeader.NalUnitType == NalUnitType.IDR)
            {
                flvTags.VideoTagsData.FrameType = FrameType.KeyFrame;
            }
            else
            {
                flvTags.VideoTagsData.FrameType = FrameType.InterFrame;
            }
            flvTags.VideoTagsData.VideoData.CompositionTime = nALU.LastFrameInterval;
            flvTags.VideoTagsData.VideoData.MultiData = new List<Byte[]>
            {
                nALU.RawData
            };
            //忽略sei
            //if (sei != null && sei.RawData != null && sei.RawData.Length > 0)
            //{
            //    flvTags.VideoTagsData.VideoData.MultiData.Add(sei.RawData);
            //}
            flvMessagePackWriter.WriteFlvTag(flvTags);
            flvMessagePackWriter.WriteUInt32((UInt32)(flvTags.DataSize + 11));
            return flvMessagePackWriter.FlushAndGetArray();
        }
        finally
        {
            FlvArrayPool.Return(buffer);
        }
    }

    private Byte[] EncoderAacAudioTag(UInt32 timestamp, Byte[] aacFrameData)
    {
        var buffer = FlvArrayPool.Rent(aacFrameData.Length);
        try
        {
            var flvMessagePackWriter = new FlvMessagePackWriter(buffer);
            //flv body audio tag
            //flv body tag header
            var flvTags = new FlvTags
            {
                Type = TagType.Audio,
                Timestamp = timestamp,
                TimestampExt = 0,
                StreamId = 0,
                //flv body tag body
                AudioTagsData = new AudioTags(AACPacketType.AudioFrame, aacFrameData)
            };
            flvMessagePackWriter.WriteFlvTag(flvTags);
            flvMessagePackWriter.WriteUInt32((UInt32)(flvTags.DataSize + 11));
            return flvMessagePackWriter.FlushAndGetArray();
        }
        finally
        {
            FlvArrayPool.Return(buffer);
        }
    }
}
