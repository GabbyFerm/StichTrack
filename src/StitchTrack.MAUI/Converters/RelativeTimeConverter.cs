using System.Globalization;

namespace StitchTrack.MAUI.Converters;

/// <summary>
/// Converts DateTime to relative time string (e.g., "2 minutes ago", "3 days ago").
/// Used for displaying when projects were last updated.
/// </summary>
public class RelativeTimeConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DateTime dateTime)
        {
            return "Unknown";
        }

        var timeSpan = DateTime.UtcNow - dateTime.ToUniversalTime();

        // Less than 1 minute
        if (timeSpan.TotalMinutes < 1)
        {
            return "Just now";
        }

        // Less than 1 hour
        if (timeSpan.TotalHours < 1)
        {
            int minutes = (int)timeSpan.TotalMinutes;
            return minutes == 1 ? "1 minute ago" : $"{minutes} minutes ago";
        }

        // Less than 24 hours
        if (timeSpan.TotalDays < 1)
        {
            int hours = (int)timeSpan.TotalHours;
            return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
        }

        // Less than 7 days
        if (timeSpan.TotalDays < 7)
        {
            int days = (int)timeSpan.TotalDays;
            return days == 1 ? "Yesterday" : $"{days} days ago";
        }

        // Less than 30 days
        if (timeSpan.TotalDays < 30)
        {
            int weeks = (int)(timeSpan.TotalDays / 7);
            return weeks == 1 ? "1 week ago" : $"{weeks} weeks ago";
        }

        // Less than 365 days
        if (timeSpan.TotalDays < 365)
        {
            int months = (int)(timeSpan.TotalDays / 30);
            return months == 1 ? "1 month ago" : $"{months} months ago";
        }

        // More than 1 year
        int years = (int)(timeSpan.TotalDays / 365);
        return years == 1 ? "1 year ago" : $"{years} years ago";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException("RelativeTimeConverter does not support ConvertBack");
    }
}