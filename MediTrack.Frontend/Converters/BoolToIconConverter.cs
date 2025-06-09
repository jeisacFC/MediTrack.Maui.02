using System.Globalization;

namespace MediTrack.Frontend.Converters
{
    public class BoolToIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 3)
                return values?[1]; // valor por defecto

            if (values[0] is bool condition && values[1] is string trueValue && values[2] is string falseValue)
            {
                return condition ? trueValue : falseValue;
            }

            return values[1]; // valor por defecto
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}