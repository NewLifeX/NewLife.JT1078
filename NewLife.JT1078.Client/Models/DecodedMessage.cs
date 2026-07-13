using NewLife.JT1078.Protocols;

namespace NewLife.JT1078.Client.Models;

/// <summary>解码后的协议指令消息</summary>
public class DecodedMessage
{
    /// <summary>消息类型标识</summary>
    public MessageKinds Kind { get; set; }

    /// <summary>消息类型名称</summary>
    public String KindName { get; set; } = "";

    /// <summary>消息说明</summary>
    public String Description { get; set; } = "";

    /// <summary>解析出的字段列表</summary>
    public List<DecodedField> Fields { get; set; } = [];

    /// <summary>原始载荷数据（Hex）</summary>
    public String? RawHex { get; set; }

    /// <summary>是否可以解析（True=已知类型且成功解码，False=未知类型）</summary>
    public Boolean IsParsed { get; set; }
}

/// <summary>解码后的字段</summary>
public class DecodedField
{
    /// <summary>字段名称</summary>
    public String Name { get; set; } = "";

    /// <summary>字段值（字符串表示）</summary>
    public String Value { get; set; } = "";

    /// <summary>原始字节（Hex）</summary>
    public String? RawHex { get; set; }

    /// <summary>字段说明</summary>
    public String? Description { get; set; }
}
