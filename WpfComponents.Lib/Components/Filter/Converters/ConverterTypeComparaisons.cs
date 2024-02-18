using System;
using System.Collections.Generic;
using System.Windows.Data;
using UltraFiltre.Lib;

namespace UltraFiltre.Wpf.Lib.Converters
{
    public class ConverterTypeComparaisons : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var lItem = value as Propriete;
            if (lItem == null)
                return new List<EnumComparaisonFiltre>();

            var lComparaisons = new List<EnumComparaisonFiltre>();
            if (lItem.Type == typeof(string))
            {
                lComparaisons.Add(EnumComparaisonFiltre.Contains);
                lComparaisons.Add(EnumComparaisonFiltre.NotContains);
                lComparaisons.Add(EnumComparaisonFiltre.EqualsTo);
                lComparaisons.Add(EnumComparaisonFiltre.NotEqualsTo);
                lComparaisons.Add(EnumComparaisonFiltre.StartsWith);
                lComparaisons.Add(EnumComparaisonFiltre.NotStartsWith);
                lComparaisons.Add(EnumComparaisonFiltre.EndsWith);
                lComparaisons.Add(EnumComparaisonFiltre.NotEndsWith);
            }
            else if (Utils.EstEntier(lItem.Type) || Utils.EstDecimal(lItem.Type) || lItem.Type == typeof(DateTime) || lItem.Type == typeof(TimeSpan))
            {
                lComparaisons.Add(EnumComparaisonFiltre.EqualsTo);
                lComparaisons.Add(EnumComparaisonFiltre.NotEqualsTo);
                lComparaisons.Add(EnumComparaisonFiltre.GreaterThan);
                lComparaisons.Add(EnumComparaisonFiltre.GreaterThanOrEqual);
                lComparaisons.Add(EnumComparaisonFiltre.LesserThan);
                lComparaisons.Add(EnumComparaisonFiltre.LesserThanOrEqual);
            }
            else
            {
                lComparaisons.Add(EnumComparaisonFiltre.EqualsTo);
                lComparaisons.Add(EnumComparaisonFiltre.NotEqualsTo);
            }

            return lComparaisons;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
