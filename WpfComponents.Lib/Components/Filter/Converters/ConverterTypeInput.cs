using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using UltraFiltre.Lib;

namespace UltraFiltre.Wpf.Lib.Converters
{
    /// <summary>
    /// Permet de sélectionné un template en fonction d'un type de données.
    /// A savoir qu'il est possible d'utilisé ContentPresenter.ContentTemplateSelector pour ça mais ça ne se refresh pas automatiquement (contrairement au Converter).
    /// </summary>
    public class ConverterTypeInput : IValueConverter
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate EntierTemplate { get; set; }
        public DataTemplate DecimalTemplate { get; set; }
        public DataTemplate BoolTemplate { get; set; }
        public DataTemplate DateTimeTemplate { get; set; }
        public DataTemplate TimeTemplate { get; set; }
        public DataTemplate EnumTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var lItem = value as Propriete;
            if (lItem != null)
            {
                if (lItem.Type.IsEnum)
                    return EnumTemplate;
                else if (Utils.EstEntier(lItem.Type))
                    return EntierTemplate;
                else if (Utils.EstDecimal(lItem.Type))
                    return DecimalTemplate;
                else if (lItem.Type == typeof(bool))
                    return BoolTemplate;
                else if (lItem.Type == typeof(DateTime))
                    return DateTimeTemplate;
                else if (lItem.Type == typeof(TimeSpan))
                    return TimeTemplate;
            }
            return StringTemplate;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
