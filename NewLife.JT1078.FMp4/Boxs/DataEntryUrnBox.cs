using NewLife.JT1078.FMp4.MessagePack;

namespace NewLife.JT1078.FMp4
{
    /// <summary></summary>
    public class DataEntryUrnBox : DataEntryBox
    {
        public DataEntryUrnBox(Byte version, UInt32 flags) : base("urn ", version, flags) { }

        public DataEntryUrnBox(UInt32 flags) : this(0, flags) { }

        public String Name { get; set; }

        public String Location { get; set; }

        public override void ToBuffer(ref FMp4MessagePackWriter writer)
        {
            Start(ref writer);
            if (!String.IsNullOrEmpty(Name))
            {
                writer.WriteUTF8(Name);
            }
            if (!String.IsNullOrEmpty(Location))
            {
                writer.WriteUTF8(Location);
            }
            End(ref writer);
        }
    }
}