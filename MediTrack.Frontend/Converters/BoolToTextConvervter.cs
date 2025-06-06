using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MediTrack.Frontend.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isLoading = (bool)value;
            var texts = parameter?.ToString()?.Split('|');

            if (texts != null && texts.Length == 2)
            {
                return isLoading ? texts[0] : texts[1]; // "Registrando..." : "Registrarse"
            }

            return isLoading ? "Cargando..." : "Continuar";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}