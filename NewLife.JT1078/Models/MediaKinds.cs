namespace NewLife.JT1078.Models
{
    /// <summary>媒体数据类型</summary>
    public enum MediaKinds : Byte
    {
        /// <summary>音视频</summary>
        AudioVideo = 0,

        /// <summary>视频</summary>
        Video = 1,

        /// <summary>双向对讲</summary>
        TwoWayTalk = 2,

        /// <summary>监听</summary>
        Monitor = 3,

        /// <summary>中心广播</summary>
        CentralBroadcast = 4,

        /// <summary>透传</summary>
        Transparent = 5,
    }
}