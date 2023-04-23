using System;
using System.Collections.Generic;

namespace NewLife.JT1078.FMp4
{
    public class SubTrackInformationBox : FullBox
    {
        public SubTrackInformationBox(Byte version = 0, UInt32 flags = 0) : base("stri", version, flags) { }

        public UInt16 SwitchGroup { get; set; }

        public UInt16 AlternateGroup { get; set; }

        public UInt32 SubTrackID { get; set; }

        public List<UInt32> AttributeList { get; set; }
    }
}