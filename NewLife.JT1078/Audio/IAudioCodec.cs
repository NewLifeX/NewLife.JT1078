namespace NewLife.JT1078.Audio
{
    public interface IAudioCodec
    {
        Byte[] ToPcm(ReadOnlySpan<Byte> audio, IAudioAttachData audioAttachData);

        Byte[] FromPcm(ReadOnlySpan<Byte> pcm, IAudioAttachData audioAttachData);
    }
}