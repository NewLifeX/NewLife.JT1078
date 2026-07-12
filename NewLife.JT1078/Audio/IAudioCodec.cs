namespace NewLife.JT1078.Audio
{
    /// <summary>音频编解码器接口。定义音频数据与 PCM 之间的双向转换</summary>
    public interface IAudioCodec
    {
        /// <summary>将音频数据解码为 PCM 格式</summary>
        /// <param name="audio">编码后的音频数据（如 G711A/U、ADPCM 等）</param>
        /// <param name="audioAttachData">附加数据，用于传递解码器状态（如 ADPCM 的 step/index）</param>
        /// <returns>解码后的 PCM 数据</returns>
        Byte[] ToPcm(ReadOnlySpan<Byte> audio, IAudioAttachData audioAttachData);

        /// <summary>将 PCM 数据编码为音频格式</summary>
        /// <param name="pcm">原始 PCM 音频数据</param>
        /// <param name="audioAttachData">附加数据，用于传递编码器状态</param>
        /// <returns>编码后的音频数据</returns>
        Byte[] FromPcm(ReadOnlySpan<Byte> pcm, IAudioAttachData audioAttachData);
    }
}