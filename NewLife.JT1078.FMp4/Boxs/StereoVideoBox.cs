using System;
using System.Collections.Generic;

namespace JT1078.FMp4;

public class StereoVideoBox : FullBox
{
    public StereoVideoBox(Byte version = 0, UInt32 flags = 0) : base("stvi", version, flags)
    {
    }
    /// <summary>
    ///4btye 32 - 30
    /// </summary>
    public UInt32 Reserved { get; set; }
    /// <summary>
    ///4btye 32 - 2
    /// </summary>
    public Byte SingleViewAllowed { get; set; }
    public UInt32 StereoScheme { get; set; }
    public UInt32 Length { get; set; }
    /// <summary>
    /// length:Length
    /// </summary>
    public Byte[] StereoIndicationType { get; set; }
    public List<Mp4Box> AnyBox { get; set; }
}
