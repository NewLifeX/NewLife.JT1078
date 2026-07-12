using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Models
{
    /// <summary>终端上传音视频熟悉</summary>
    [MessageKind(MessageKinds.UploadAVAttribute)]
    public class T1003
    {
        /// <summary>音频编码方式</summary>
        public AudioCodecs AudioCodec { get; set; }

        /// <summary>音频声道数</summary>
        public Byte AudioChannels { get; set; }

        /// <summary>音频采样率。0：8 kHz； 1：22.05 kHz； 2：44.1 kHz； 3：48 kHz</summary>
        public AudioRates AudioRate { get; set; }

        /// <summary>音频采样位数。0：8 位；1：16 位；2：32 位</summary>
        public AudioBits AudioBit { get; set; }

        /// <summary>音频帧长度。范围 1～4,294,967,295</summary>
        public UInt16 AudioFrameLength { get; set; }

        /// <summary>是否支持音频输出。0：不支持；1：支持</summary>
        public Byte AudioOutput { get; set; }

        /// <summary>视频编码方式</summary>
        public VideoCodecs VideoCodec { get; set; }

        /// <summary>终端支持的最大音频物理通道数量</summary>
        public Byte MaxAudioChannels { get; set; }

        /// <summary>终端支持的最大视频物理通道数量</summary>
        public Byte MaxVideoChannels { get; set; }
    }
}