using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace WpfComponents.Lib.Converters
{
    public class UniversalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            UniversalBoolConverter boolConverter = new UniversalBoolConverter();
            bool isVisible = (bool)boolConverter.Convert(value, targetType, parameter, culture);
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class MultiUniversalVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            UniversalBoolConverter boolConverter = new UniversalBoolConverter();
            bool isVisible = (bool)boolConverter.Convert(values, targetType, parameter, culture);
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}