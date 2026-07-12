namespace NewLife.JT1078.Audio
{
    /// <summary>音频附加数据接口。标记接口，用于在编解码链中传递编解码器状态信息</summary>
    /// <remarks>
    /// 具体实现类包含编解码器所需的中间状态，如 ADPCM 的 step/index 等。
    /// 各编解码器通过此接口传递其特有的状态数据，无需统一抽象。
    /// </remarks>
    public interface IAudioAttachData { }
}