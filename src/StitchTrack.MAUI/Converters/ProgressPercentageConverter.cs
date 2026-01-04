using System.Globalization;
using StitchTrack.Domain.Entities;

namespace StitchTrack.MAUI.Converters;

/// <summary>
/// Calculates progress percentage from a Project entity.
/// </summary>
public class ProgressPercentageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Project project)
            return "0";

        if (!project.TotalRows.HasValue || project.TotalRows.Value == 0)
            return "0";

        var percentage = (int)Math.Round((double)project.CurrentCount / project.TotalRows.Value * 100);
        return Math.Min(percentage, 100).ToString(); // Cap at 100%
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
