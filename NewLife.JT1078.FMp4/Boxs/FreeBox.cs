namespace NewLife.JT1078.FMp4
{
    /// <summary>空闲 Box。用于文件填充或占位</summary>
    public class FreeBox : FreeSpaceBox
    {
        public FreeBox() : base("free") { }
    }
}
