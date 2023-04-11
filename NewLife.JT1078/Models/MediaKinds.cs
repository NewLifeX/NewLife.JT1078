namespace NewLife.JT1078.Models
{
    /// <summary>媒体数据类型</summary>
    public enum MediaKinds : Byte
    {
        /// <summary>音视频</summary>
        音视频 = 0,

        /// <summary>视频</summary>
        视频 = 1,

        /// <summary>双向对讲</summary>
        双向对讲 = 2,

        /// <summary>监听</summary>
        监听 = 3,

        /// <summary>中心广播</summary>
        中心广播 = 4,

        /// <summary>子码流</summary>
        透传 = 5,
    }
}