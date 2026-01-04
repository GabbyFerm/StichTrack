using System.Globalization;

namespace StitchTrack.MAUI.Converters;

public class TotalRowsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int totalRows && totalRows > 0)
        {
            return $" of {totalRows}";
        }
        return string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
