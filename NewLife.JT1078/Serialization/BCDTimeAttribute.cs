using System.Globalization;
using NewLife.Reflection;
using NewLife.Serialization;

namespace NewLife.JT1078.Serialization;

/// <summary>BCD时间序列化特性</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class BCDTimeAttribute : AccessorAttribute
{
    /// <summary>长度</summary>
    public Int32 Length { get; set; }

    /// <summary>格式化字符串</summary>
    public String Format { get; set; } = "yyMMddHHmmss";

    /// <summary>>BCD时间序列化</summary>
    /// <param name="length"></param>
    /// <param name="format"></param>
    public BCDTimeAttribute(Int32 length, String format)
    {
        Length = length;
        Format = format;
    }

    /// <summary>从数据流中读取消息</summary>
    /// <param name="formatter">序列化</param>
    /// <param name="context">上下文</param>
    /// <returns>是否成功</returns>
    public override Boolean Read(IFormatterX formatter, AccessorContext context)
    {
        if (formatter is not Binary bn) return false;

        var str = bn.ReadBCD(Length);
        if (DateTime.TryParseExact(str, Format, null, DateTimeStyles.None, out var dt))
            context.Value.SetValue(context.Member, dt);

        return true;
    }

    /// <summary>把消息写入到数据流中</summary>
    /// <param name="formatter">序列化</param>
    /// <param name="context">上下文</param>
    public override Boolean Write(IFormatterX formatter, AccessorContext context)
    {
        if (formatter is not Binary bn) return false;

        var dt = (DateTime)context.Value.GetValue(context.Member);
        var str = dt.ToString(Format);
        bn.WriteBCD(str, Length);

        return true;
    }
}