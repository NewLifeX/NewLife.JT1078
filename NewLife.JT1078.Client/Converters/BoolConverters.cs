using Avalonia.Data.Converters;

namespace NewLife.JT1078.Client.Converters;

/// <summary>布尔值转换器</summary>
public static class BoolConverters
{
    /// <summary>逻辑非转换器</summary>
    public static readonly IValueConverter Not = new NotConverter();
}

/// <summary>逻辑非转换器实现</summary>
public class NotConverter : IValueConverter
{
    /// <summary>转换</summary>
    public Object? Convert(Object? value, Type targetType, Object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Boolean b) return !b;
        return value;
    }

    /// <summary>反向转换</summary>
    public Object? ConvertBack(Object? value, Type targetType, Object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is Boolean b) return !b;
        return value;
    }
}
