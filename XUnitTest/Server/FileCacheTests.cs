using NewLife.JT1078.Server;
using Xunit;

namespace XUnitTest.Server;

public class FileCacheTests : IDisposable
{
    private readonly String _tmpDir;

    public FileCacheTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "JT1078_Test_" + Guid.NewGuid().ToString("N"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tmpDir))
            Directory.Delete(_tmpDir, true);
    }

    [Fact(DisplayName = "FileCache 写入和完成")]
    public void FileCache_WriteFinish()
    {
        using var cache = new FileCache(_tmpDir);

        cache.Write("flv", "138001380000", 1, new Byte[] { 0x46, 0x4C, 0x56 });
        cache.Finish("flv", "138001380000", 1);

        var files = cache.GetFiles("138001380000", 1);
        Assert.NotEmpty(files);
        Assert.Single(files);
    }

    [Fact(DisplayName = "FileCache 多类型写入")]
    public void FileCache_MultiType()
    {
        using var cache = new FileCache(_tmpDir);

        cache.Write("flv", "138001380000", 1, new Byte[] { 0x46 });
        cache.Write("pcm", "138001380000", 1, new Byte[] { 0x50 });
        cache.Finish("flv", "138001380000", 1);
        cache.Finish("pcm", "138001380000", 1);

        var files = cache.GetFiles("138001380000", 1);
        Assert.Equal(2, files.Length);
    }

    [Fact(DisplayName = "FileCache 不同日期目录")]
    public void FileCache_DateDir()
    {
        using var cache = new FileCache(_tmpDir);

        cache.Write("flv", "138001380000", 1, new Byte[] { 0x46 });
        cache.Finish("flv", "138001380000", 1);

        var today = DateTime.Now.ToString("yyyyMMdd");
        var dir = Path.Combine(_tmpDir, today);
        Assert.True(Directory.Exists(dir));
    }
}
