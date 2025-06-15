//using System.Globalization;
//using System.Collections.ObjectModel;
//using MediTrack.Frontend.Models.Model;

//namespace MediTrack.Frontend.Converters
//{
//    public class CompletedEventsCountConverter : IValueConverter
//    {
//        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            if (value is ObservableCollection<EventosMedicos> eventos)
//            {
//                return eventos.Count(e => e.Completado);
//            }
//            return 0;
//        }

//        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}