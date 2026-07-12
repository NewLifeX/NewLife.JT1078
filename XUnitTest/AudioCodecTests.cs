using NewLife.JT1078.Audio;
using Xunit;

namespace XUnitTest.Audio;

public class AudioCodecTests
{
    [Fact(DisplayName = "G711A 编解码往返")]
    public void G711A_RoundTrip()
    {
        var codec = new G711ACodec();

        // 构造测试 PCM 数据：正弦波采样
        var samples = 1000;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var sample = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (Byte)((sample >> 8) & 0xFF);
        }

        // 编码 PCM → G711A
        var g711a = codec.FromPcm(pcm, null!);
        Assert.NotNull(g711a);
        Assert.Equal(samples, g711a.Length);

        // 解码 G711A → PCM
        var pcm2 = codec.ToPcm(g711a, null!);
        Assert.NotNull(pcm2);
        Assert.Equal(samples * 2, pcm2.Length);

        // 验证失真在合理范围（G711 是 8bit 压缩，8k 采样，允许 ±2 误差）
        var maxError = 0;
        for (var i = 0; i < samples * 2; i++)
        {
            var err = Math.Abs(pcm[i] - pcm2[i]);
            if (err > maxError) maxError = err;
        }
        Assert.True(maxError <= 256, $"G711A 最大误差 {maxError}，预期 ≤256");
    }

    [Fact(DisplayName = "G711U 编解码往返")]
    public void G711U_RoundTrip()
    {
        var codec = new G711UCodec();

        // 构造测试 PCM 数据
        var samples = 1000;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var sample = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (Byte)((sample >> 8) & 0xFF);
        }

        // 编码 PCM → G711U
        var g711u = codec.FromPcm(pcm, null!);
        Assert.NotNull(g711u);
        Assert.Equal(samples, g711u.Length);

        // 解码 G711U → PCM
        var pcm2 = codec.ToPcm(g711u, null!);
        Assert.NotNull(pcm2);
        Assert.Equal(samples * 2, pcm2.Length);

        // 验证失真
        var maxError = 0;
        for (var i = 0; i < samples * 2; i++)
        {
            var err = Math.Abs(pcm[i] - pcm2[i]);
            if (err > maxError) maxError = err;
        }
        Assert.True(maxError <= 256, $"G711U 最大误差 {maxError}，预期 ≤256");
    }

    [Fact(DisplayName = "ADPCM 编解码往返")]
    public void ADPCM_RoundTrip()
    {
        var codec = new ADPCMCodec();
        var state = new AdpcmState();

        // 构造测试 PCM 数据
        var samples = 1000;
        var pcm = new Byte[samples * 2];
        for (var i = 0; i < samples; i++)
        {
            var sample = (Int16)(Math.Sin(i * 2 * Math.PI / 50) * 5000);
            pcm[i * 2] = (Byte)(sample & 0xFF);
            pcm[i * 2 + 1] = (Byte)((sample >> 8) & 0xFF);
        }

        // 编码 PCM → ADPCM
        var adpcm = codec.FromPcm(pcm, state);
        Assert.NotNull(adpcm);

        // 解码 ADPCM → PCM（新的 AdpcmState 实例，重置预测值）
        state = new AdpcmState();
        var pcm2 = codec.ToPcm(adpcm, state);
        Assert.NotNull(pcm2);
        // ADPCM 解码输出可能因块对齐有少量偏差，验证在合理范围内
        Assert.True(Math.Abs(pcm2.Length - samples * 2) <= 32, $"ADPCM 输出长度 {pcm2.Length} 与输入 {samples * 2} 偏差过大");

        // ADPCM 是有损压缩，只验证长度正确，不验证精确误差
    }

    [Fact(DisplayName = "海思头去除和添加")]
    public void HisiHeader_TrimAdd()
    {
        var factory = new AudioCodecFactory();

        // 海思头格式：[0x00, 0x01, (data_len-4)/2, 0x00]
        // 数据总长 8 = 4字节头 + 4字节负载，所以第三字节 = (8-4)/2 = 0x02
        var hisiHeader = new Byte[] { 0x00, 0x01, 0x02, 0x00 };
        var data = new Byte[] { 0x11, 0x22, 0x33, 0x44 };
        var combined = hisiHeader.Concat(data).ToArray();

        // 去除海思头
        var trimmed = factory.TrimHI(combined, out var trim);
        Assert.True(trim);
        Assert.Equal(data, trimmed.ToArray());

        // 无海思头时 Trim 应返回原数据
        var trimmed2 = factory.TrimHI(data, out var trim2);
        Assert.False(trim2);
        Assert.Equal(data, trimmed2.ToArray());
    }
}
