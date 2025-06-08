using System.Globalization;

namespace MediTrack.Frontend.Converters
{
    public class TipoEventoToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string tipo)
            {
                return tipo.ToLower() switch
                {
                    "medicamento" or "medicacion" => Color.FromArgb("#2196F3"),    // Azul
                    "cita médica" or "cita medica" or "cita" => Color.FromArgb("#FF5722"),    // Rojo-naranja
                    "análisis" or "analisis" => Color.FromArgb("#E91E63"),       // Rosa
                    "recordatorio" => Color.FromArgb("#9C27B0"),   // Morado
                    "ejercicio" => Color.FromArgb("#4CAF50"),      // Verde
                    _ => Color.FromArgb("#607D8B")                 // Gris por defecto
                };
            }
            return Color.FromArgb("#607D8B"); // Gris por defecto
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}