namespace NewLife.JT1078.Models;

public enum CloseKinds : Byte
{
    /// <summary>关闭该通道有关的音视频数据</summary>
    AudioAndVideo = 0,

    /// <summary>只关闭该通道有关的音频，保留该通道有关的视频</summary>
    Audio = 1,

    /// <summary>只关闭该通道有关的视频，保留该通道有关的音频</summary>
    Video = 2,
}