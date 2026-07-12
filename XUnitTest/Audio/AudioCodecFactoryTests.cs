using NewLife.JT1078.Audio;
using NewLife.JT1078.Protocols;
using Xunit;

namespace XUnitTest.Audio;

public class AudioCodecFactoryTests
{
    [Fact(DisplayName = "工厂 ToPcm G711A 分发")]
    public void Factory_ToPcm_G711A()
    {
        var factory = new AudioCodecFactory();
        var codec = new G711ACodec();

        var samples = 100;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        // PCM → G711A → PCM via factory
        var g711a = codec.FromPcm(pcm, null!);
        var decoded = factory.ToPcm(AVTypes.G711A, g711a);

        Assert.NotNull(decoded);
        Assert.Equal(samples * 2, decoded.Length);
    }

    [Fact(DisplayName = "工厂 ToPcm G711U 分发")]
    public void Factory_ToPcm_G711U()
    {
        var factory = new AudioCodecFactory();
        var codec = new G711UCodec();

        var samples = 100;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        var g711u = codec.FromPcm(pcm, null!);
        var decoded = factory.ToPcm(AVTypes.G711U, g711u);

        Assert.NotNull(decoded);
        Assert.Equal(samples * 2, decoded.Length);
    }

    [Fact(DisplayName = "工厂 ToPcm ADPCMA 分发")]
    public void Factory_ToPcm_ADPCMA()
    {
        var factory = new AudioCodecFactory();
        var codec = new ADPCMCodec();

        var samples = 200;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        var state = new AdpcmState();
        var adpcm = codec.FromPcm(pcm, state);
        var decoded = factory.ToPcm(AVTypes.ADPCMA, adpcm);

        Assert.NotNull(decoded);
        Assert.True(decoded.Length > 0);
    }

    [Fact(DisplayName = "工厂 FromPcm G711A 分发")]
    public void Factory_FromPcm_G711A()
    {
        var factory = new AudioCodecFactory();

        var samples = 100;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        var encoded = factory.FromPcm(AVTypes.G711A, pcm);
        Assert.NotNull(encoded);
        Assert.Equal(samples, encoded.Length);
    }

    [Fact(DisplayName = "工厂 海思头+编解码往返")]
    public void Factory_HisiRoundTrip()
    {
        var factory = new AudioCodecFactory();

        var samples = 200;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var s = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(s & 0xFF);
            pcm[i * 2 + 1] = (Byte)((s >> 8) & 0xFF);
        }

        // 通过工厂编码 + 海思头
        var encoded = factory.FromPcm(AVTypes.G711A, pcm);
        Assert.NotNull(encoded);

        // 工厂标注了海思头后，解码应能正确去除
        var decoded = factory.ToPcm(AVTypes.G711A, encoded);
        Assert.NotNull(decoded);
    }

    [Fact(DisplayName = "工厂 不支持的编码类型抛异常")]
    public void Factory_NotSupported()
    {
        var factory = new AudioCodecFactory();

        Assert.Throws<NotSupportedException>(() => factory.ToPcm(AVTypes.H264, new Byte[10]));
        Assert.Throws<NotSupportedException>(() => factory.FromPcm(AVTypes.H264, new Byte[10]));
    }

    [Fact(DisplayName = "工厂 AddHI 添加海思头")]
    public void Factory_AddHI()
    {
        var factory = new AudioCodecFactory();
        var data = new Byte[] { 0x11, 0x22, 0x33, 0x44 };
        var result = factory.AddHI(data);

        Assert.Equal(8, result.Length);
        Assert.Equal(0x00, result[0]);
        Assert.Equal(0x01, result[1]);
        Assert.Equal(0x02, result[2]); // data.Length/2 = 2
        Assert.Equal(0x00, result[3]);
        Assert.Equal(0x11, result[4]);
    }
}
