namespace NewLife.JT1078.Models
{
    /// <summary>流指令</summary>
    public enum StreamCommands : Byte
    {
        /// <summary>关闭音视频传输指令</summary>
        CloseAudioAndVideo,

        /// <summary>切换码流（增加暂停和继续）</summary>
        Switch,

        /// <summary>暂停该通道所有流的发送</summary>
        Pause,

        /// <summary>恢复暂停前流的发送，与暂停前的流类型一致</summary>
        Resume,

        /// <summary>关闭双向对讲</summary>
        CloseTalkback,
    }
}