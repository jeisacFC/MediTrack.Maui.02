using System.Globalization;

namespace MediTrack.Frontend.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool completado)
            {
                return completado ? Color.FromArgb("#34C759") : Color.FromArgb("#FF9500"); // Verde iOS y Naranja iOS
            }
            return Color.FromArgb("#8E8E93"); // Gris iOS
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}