using System;
using System.Collections.Generic;
using JT1078.FMp4.Samples;

namespace JT1078.FMp4;

public class SampleGroupDescriptionBox : FullBox
{
    public SampleGroupDescriptionBox(String handlerType, Byte version, UInt32 flags = 0) : base("sgpd", version, flags) => HandlerType = handlerType;

    public String HandlerType { get; set; }
    /// <summary>
    /// if (version==1)
    /// </summary>
    public UInt32 DefaultLength { get; set; }
    public UInt32 EntryCount { get; set; }
    public List<SampleGroupDescription> SampleGroupDescriptions { get; set; }
    public class SampleGroupDescription
    {
        public UInt32 DescriptionLength { get; set; }

        public SampleGroupDescriptionEntry SampleGroupDescriptionEntry { get; set; }
    }
}
