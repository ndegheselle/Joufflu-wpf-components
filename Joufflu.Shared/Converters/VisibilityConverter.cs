using System.Windows;
using System.Windows.Data;

namespace Joufflu.Shared.Converters
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BooleanConverter lConverterBool = new BooleanConverter();
            bool lVisible = (bool)lConverterBool.Convert(value, targetType, parameter, culture);
            return lVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }
}
