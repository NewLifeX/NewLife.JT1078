using NewLife.JT1078.Flv.Enums;

namespace NewLife.JT1078.Flv.Metadata
{
    public class VideoTags
    {
        /// <summary>
        /// 高4位
        /// 1: keyframe(for AVC, a seekable frame) —— 即H.264的IDR帧；
        /// 2: inter frame(for AVC, a non- seekable frame) —— H.264的普通I帧；
        /// </summary>
        public FrameType FrameType { get; set; }

        /// <summary>
        /// 第四位
        /// 当 CodecID 为 7 时，VideoData 为 AVCVIDEOPACKE，也即 H.264媒体数据
        /// </summary>
        public CodecId CodecId { get; set; } = CodecId.AvcVideoPacke;

        /// <summary>视频数据</summary>
        public AvcVideoPacke VideoData { get; set; }
    }
}
