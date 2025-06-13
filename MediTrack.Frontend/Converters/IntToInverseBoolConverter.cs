using System.Globalization;

namespace MediTrack.Frontend.Converters
{
    public class IntToInverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
                return intValue == 0; // TRUE cuando es 0 (vacío), FALSE cuando tiene elementos
            return true; // Por defecto mostrar mensaje de vacío
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return boolValue ? 0 : 1;
            return 0;
        }
    }
}