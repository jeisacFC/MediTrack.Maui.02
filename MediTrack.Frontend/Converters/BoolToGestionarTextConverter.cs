using System.Globalization;

namespace MediTrack.Frontend.Converters
{
    public class BoolToGestionarTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool haySintomas)
                return haySintomas ? "Gestionar" : "¿Cómo te sientes?";
            return "Gestionar";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToButtonWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool haySintomas)
                return haySintomas ? 75 : 120; // Más ancho para "¿Cómo te sientes?"
            return 75;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}