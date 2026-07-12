namespace NewLife.JT1078.FMp4
{
    /// <summary>跳过 Box。与 FreeBox 类似，用于文件填充</summary>
    public class SkipBox : FreeSpaceBox
    {
        public SkipBox() : base("skip") { }
    }
}