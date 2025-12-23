using System.Globalization;

namespace ArunayanDairy.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue || parameter is not string paramString)
            return Colors.Gray;

        var parts = paramString.Split('|');
        if (parts.Length != 2)
            return Colors.Gray;

        var colorString = boolValue ? parts[0] : parts[1];
        return Color.FromArgb(colorString);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
