using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfComponents.Lib.Converters
{
    /// <summary>
    /// Converts any property to a boolean value
    /// 3ma/page/converters
    /// </summary>
    public class UniversalBoolConverter : IValueConverter
    {
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">bool, indicates whether the result should be true or false to be visible</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool result = false;

            if (value is bool)
                result = (bool)value;
            else if (value is string)
                result = !string.IsNullOrEmpty(value as string);
            else if (value is int)
                result = (int)value > 0;
            else if (value is ICollection)
                result = ((ICollection)value).Count > 0;
            else
                result = value != null;

            // Inverts the result if ConverterParam=False, more logical than simply inverting
            // For example, a converter {IsEnabled, ConverterParameter=False} allows to have the equivalent of IsEnabled == false
            bool target;
            if (!bool.TryParse(parameter?.ToString(), out target))
                target = true;

            return result == target;
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Converts a list of properties to a boolean value (And / Or operator as parameter)
    /// It is possible to use the ConverterBooleanInverse converter in the MultiBinding bindings
    /// </summary>
    public class MultiUniversalBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            UniversalBoolConverter converter = new UniversalBoolConverter();
            bool result = true;

            foreach (object value in values)
            {
                result = result && (bool)converter.Convert(value, targetType, parameter, culture);
            }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
