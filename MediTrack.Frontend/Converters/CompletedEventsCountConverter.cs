using System.Globalization;
using System.Collections.ObjectModel;
using MediTrack.Frontend.Models.Response;

namespace MediTrack.Frontend.Converters
{
    public class CompletedEventsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<ResEventoCalendario> eventos)
            {
                // Como ResEventoCalendario no tiene campo "Completado", 
                // simular que los eventos pasados están completados
                return eventos.Count(e => e.FechaHora < DateTime.Now);
            }

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}