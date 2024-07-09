using System.Globalization;
using System.Windows.Data;

namespace Joufflu.Inputs.Components.Format
{
    // Should be possible to bind directly a MultiBinding to a object[] property ...
    public class ToListConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.ToList();
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            var list = (IEnumerable<object>)value;
            return list.ToArray();
        }
    }
}
