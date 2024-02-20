using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;

namespace WpfComponents.Lib.Components.Filters.Converters
{
    /// <summary>
    /// Permet de convertir une valeur en une autre valeur en utilisant le convertisseur par défaut
    /// Par exemple dans le cas d'un double vers un float
    /// </summary>
    public class ConverterParseValue : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            try
            {
                var converter = TypeDescriptor.GetConverter(targetType);
                return converter.ConvertFrom(null, CultureInfo.CurrentCulture, value.ToString());
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
