namespace NewLife.JT1078.Protocols
{
    /// <summary>消息类型特性</summary>
    public class MessageKindAttribute : Attribute
    {
        /// <summary>类型</summary>
        public MessageKinds Kind { get; set; }

        /// <summary>实例化消息类型特性</summary>
        /// <param name="kind"></param>
        public MessageKindAttribute(MessageKinds kind) => Kind = kind;
    }
}