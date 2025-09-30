using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace Joufflu.Shared.Converters
{
    /// <summary>
    /// Convert any value to a boolean.
    /// </summary>
    public class BooleanConverter : IValueConverter
    {
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">bool, indicate wheter the result should be true or false</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
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

            // For exemple if the parameter is "false", and the the value is null, the result will be true.
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
    /// Permet de convertir une liste de propriété en un booleen (opérateur And / Or en paramètre)
    /// Il est possible d'utilisé le converter ConverterBooleanInverse dans les Binding du MultiBinding
    /// </summary>
    public class BooleansConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string lConjonction = parameter?.ToString()?.Trim() ?? "&&";
            BooleanConverter lConverter = new BooleanConverter();
            bool lResultat = lConjonction == "&&" ? true : false;

            foreach (object lValue in values)
            {
                if (lConjonction == "&&")
                    lResultat = lResultat && (bool)lConverter.Convert(lValue, targetType, parameter, culture)!;
                else
                    lResultat = lResultat || (bool)lConverter.Convert(lValue, targetType, parameter, culture)!;
            }
            return lResultat;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
