using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using WpfComponents.Lib.Components.Filters.Data;

namespace WpfComponents.Lib.Components.Filters.Converters
{
    /// <summary>
    /// Permet de sélectionné un template en fonction d'un type de données.
    /// A savoir qu'il est possible d'utilisé ContentPresenter.ContentTemplateSelector pour ça mais ça ne se refresh pas automatiquement (contrairement au Converter).
    /// </summary>
    public class ConverterTypeInput : IValueConverter
    {
        public DataTemplate StringTemplate { get; set; }
        public DataTemplate NumericTemplate { get; set; }
        public DataTemplate DecimalTemplate { get; set; }
        public DataTemplate BoolTemplate { get; set; }
        public DataTemplate DateTimeTemplate { get; set; }
        public DataTemplate TimeTemplate { get; set; }
        public DataTemplate EnumTemplate { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = value as Property;
            if (item != null)
            {
                if (item.Type.IsEnum)
                    return EnumTemplate;
                else if (Utils.IsNumeric(item.Type))
                    return NumericTemplate;
                else if (Utils.IsDecimal(item.Type))
                    return DecimalTemplate;
                else if (item.Type == typeof(bool))
                    return BoolTemplate;
                else if (item.Type == typeof(DateTime))
                    return DateTimeTemplate;
                else if (item.Type == typeof(TimeSpan))
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
