namespace NewLife.JT1078.Protocols
{
    /// <summary>分包标记</summary>
    /// <remarks>
    /// 0000：原子包，不可被拆分
    /// 0001：分包处理时的第一个包
    /// 0010：分包处理是的最后一个包
    /// 0011：分包处理时间的中间包
    /// </remarks>
    public enum PacketFlags : Byte
    {
        /// <summary>原子包，不可被拆分</summary>
        Alone = 0,

        /// <summary>分包处理时的第一个包</summary>
        First = 1,

        /// <summary>分包处理是的最后一个包</summary>
        Last = 2,

        /// <summary>分包处理时间的中间包</summary>
        Middle = 3,
    }
}