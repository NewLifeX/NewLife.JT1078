using NewLife.JT1078.Protocols;
using NewLife.JT1078.Serialization;

namespace NewLife.JT1078.Models
{
    /// <summary>平台下发远程录像回放控制</summary>
    [MessageKind(MessageKinds.PlaybackControl)]
    public class T9202
    {
        /// <summary>逻辑通道号</summary>
        public Byte Channel { get; set; }

        /// <summary>回放控制</summary>
        /// <remarks>
        /// 0：开始回放；
        /// 1：暂停回放；
        /// 2：结束回放；
        /// 3：快进回放；
        /// 4：关键帧快退回放；
        /// 5：拖动回放；
        /// 6：关键帧播放
        /// </remarks>
        public Byte Control { get; set; }

        /// <summary>快进或快退倍数</summary>
        /// <remarks>
        /// 回放控制为 3 和 4 时，此字段内容有效，否则置 0
        /// 0：无效；
        /// 1：1 倍；
        /// 2：2 倍；
        /// 3：4 倍；
        /// 4：8 倍；
        /// 5：16 倍
        /// </remarks>
        public Byte Multiple { get; set; }

        /// <summary>拖动回放位置</summary>
        /// <remarks>回放控制为 5 时，此字段有效</remarks>
        [BCDTime(6, "yyMMddHHmmss")]
        public DateTime NewTime { get; set; }
    }
}