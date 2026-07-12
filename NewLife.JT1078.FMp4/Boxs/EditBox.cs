using NewLife.JT1078.FMp4.Interfaces;
using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary>编辑 Box。包含编辑列表的容器</summary>
    public class EditBox : Mp4Box, IFMp4MessagePackFormatter
    {
        public EditBox() : base("edts") { }

        public List<EditListBox> EditListBox { get; set; }

        public void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            End(ref writer);
        }
    }
}
