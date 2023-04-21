using System;
using System.Collections.Generic;

namespace NewLife.JT1078.FMp4
{
    public class GroupIdToNameBox : FullBox
    {
        public GroupIdToNameBox(Byte version = 0, UInt32 flags = 0) : base("gitn", version, flags) { }

        public UInt16 EntryCount { get; set; }

        public List<GroupIdToNameInfo> GroupIdToNameInfos { get; set; }

        public class GroupIdToNameInfo
        {
            public UInt32 GroupID { get; set; }

            public String GroupName { get; set; }
        }
    }
}