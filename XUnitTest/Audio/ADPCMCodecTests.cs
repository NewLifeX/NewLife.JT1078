using NewLife.JT1078.Audio;
using Xunit;

namespace XUnitTest.Audio;

public class ADPCMCodecTests
{
    [Fact(DisplayName = "ADPCM ToWav WAV 头生成")]
    public void ToWav_Header()
    {
        var pcm = new Byte[16000]; // 1 second at 8kHz
        for (var i = 0; i < pcm.Length / 2; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        var wav = pcm.ToWav(8000, 16);
        Assert.NotNull(wav);
        Assert.True(wav.Length > 44);

        // RIFF 头
        Assert.Equal(0x52, wav[0]); // R
        Assert.Equal(0x49, wav[1]); // I
        Assert.Equal(0x46, wav[2]); // F
        Assert.Equal(0x46, wav[3]); // F

        // WAVE
        Assert.Equal(0x57, wav[8]);  // W
        Assert.Equal(0x41, wav[9]);  // A
        Assert.Equal(0x56, wav[10]); // V
        Assert.Equal(0x45, wav[11]); // E

        // 数据块大小
        var dataSize = BitConverter.ToInt32(wav, 40);
        Assert.Equal(pcm.Length, dataSize);
    }

    [Fact(DisplayName = "ADPCM 编解码器连续多帧")]
    public void ADPCM_MultiFrame()
    {
        var codec = new ADPCMCodec();

        // 生成连续多帧 PCM 数据
        var samples = 2000;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 100) * 8000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        var state = new AdpcmState();

        // 分帧编码 (每帧 500 样本)
        var allAdpcm = new List<Byte>();
        for (var offset = 0; offset < samples; offset += 500)
        {
            var len = Math.Min(500, samples - offset);
            var frame = new Byte[len * 2];
            Buffer.BlockCopy(pcm, offset * 2, frame, 0, len * 2);
            var encoded = codec.FromPcm(frame, state);
            allAdpcm.AddRange(encoded);
        }

        // 连续解码
        state = new AdpcmState();
        var decoded = codec.ToPcm(allAdpcm.ToArray(), state);
        Assert.NotNull(decoded);
        Assert.True(decoded.Length > 0);
    }
}
