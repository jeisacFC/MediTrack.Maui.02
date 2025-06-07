using System.Globalization;

namespace MediTrack.Frontend.Converters
{
    public class BoolToColorManualConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool esManual)
                return esManual ? Color.FromArgb("#FF9500") : Color.FromArgb("#4CAF50");
            return Color.FromArgb("#999999");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}