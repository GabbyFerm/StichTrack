using System.Globalization;

namespace StitchTrack.MAUI.Converters
{
    public class RelativeTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var span = DateTime.Now - dateTime;
                if (span.TotalSeconds < 60)
                    return "Just now";
                if (span.TotalMinutes < 60)
                    return $"{(int)span.TotalMinutes} minutes ago";
                if (span.TotalHours < 24)
                    return $"{(int)span.TotalHours} hours ago";
                if (span.TotalDays < 7)
                    return $"{(int)span.TotalDays} days ago";
                return dateTime.ToShortDateString();
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
