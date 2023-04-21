using System;

namespace JT1078.FMp4;

public class SchemeTypeBox : FullBox
{
    public SchemeTypeBox(Byte version, UInt32 flags) : base("schm", version, flags)
    {
    }

    public SchemeTypeBox(UInt32 flags) : this(0, flags)
    {
    }

    public UInt32 SchemeType { get; set; }

    public UInt32 SchemeVersion { get; set; }
    /// <summary>
    /// if (flags & 0x000001)
    ///  UTF-8
    /// </summary>
    public String SchemeUri { get; set; }
}
