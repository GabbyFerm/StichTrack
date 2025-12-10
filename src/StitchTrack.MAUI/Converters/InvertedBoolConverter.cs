using System.Globalization;

namespace StitchTrack.MAUI.Converters;

/// <summary>
/// Converts boolean values to their inverse.
/// Used for showing/hiding UI elements based on opposite conditions.
/// Example: IsVisible="{Binding IsEmpty, Converter={StaticResource InvertedBoolConverter}}"
/// </summary>
public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}
