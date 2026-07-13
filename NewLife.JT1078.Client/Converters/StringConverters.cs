using Avalonia.Data.Converters;

namespace NewLife.JT1078.Client.Converters;

/// <summary>字符串转换器</summary>
public static class StringConverters
{
    /// <summary>字符串非空判断转换器</summary>
    public static readonly IValueConverter IsNotNullOrEmpty = new StringNotEmptyConverter();
}

/// <summary>字符串非空判断转换器实现</summary>
public class StringNotEmptyConverter : IValueConverter
{
    /// <summary>转换</summary>
    public Object? Convert(Object? value, Type targetType, Object? parameter, System.Globalization.CultureInfo culture)
    {
        return value is String s && !String.IsNullOrEmpty(s);
    }

    /// <summary>反向转换</summary>
    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotImplementedException();
}
