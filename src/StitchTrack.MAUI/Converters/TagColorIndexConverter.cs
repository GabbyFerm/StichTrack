using StitchTrack.Domain.Entities;
using System.Globalization;

namespace StitchTrack.MAUI.Converters;

public class TagColorIndexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int index)
            return Color.FromArgb(TagColors.GetColor(index));

        return Color.FromArgb("#6B7280"); // fallback grey
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
