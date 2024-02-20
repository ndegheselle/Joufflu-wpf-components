using System;
using System.Collections.Generic;
using System.Windows.Data;
using WpfComponents.Lib.Components.Filters.Data;

namespace WpfComponents.Lib.Components.Filters.Converters
{
    public class ConverterTypeComparaisons : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var item = value as Property;
            if (item == null)
                return new List<EnumOperatorFilter>();

            var comparaison = new List<EnumOperatorFilter>();
            if (item.Type == typeof(string))
            {
                comparaison.Add(EnumOperatorFilter.Contains);
                comparaison.Add(EnumOperatorFilter.NotContains);
                comparaison.Add(EnumOperatorFilter.EqualsTo);
                comparaison.Add(EnumOperatorFilter.NotEqualsTo);
                comparaison.Add(EnumOperatorFilter.StartsWith);
                comparaison.Add(EnumOperatorFilter.NotStartsWith);
                comparaison.Add(EnumOperatorFilter.EndsWith);
                comparaison.Add(EnumOperatorFilter.NotEndsWith);
            }
            else if (Utils.IsNumeric(item.Type) || Utils.IsDecimal(item.Type) || item.Type == typeof(DateTime) || item.Type == typeof(TimeSpan))
            {
                comparaison.Add(EnumOperatorFilter.EqualsTo);
                comparaison.Add(EnumOperatorFilter.NotEqualsTo);
                comparaison.Add(EnumOperatorFilter.GreaterThan);
                comparaison.Add(EnumOperatorFilter.GreaterThanOrEqual);
                comparaison.Add(EnumOperatorFilter.LesserThan);
                comparaison.Add(EnumOperatorFilter.LesserThanOrEqual);
            }
            else
            {
                comparaison.Add(EnumOperatorFilter.EqualsTo);
                comparaison.Add(EnumOperatorFilter.NotEqualsTo);
            }

            return comparaison;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
