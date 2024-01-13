using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace WpfComponents.App.Base.Converters
{
    public class UniversalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool visible = false;

            if (value is bool)
                visible = (bool)value;
            else if (value is string)
                visible = !string.IsNullOrEmpty(value as string);
            else if (value is int)
                visible = (int)value > 0;
            else
                visible = value != null;

            bool.TryParse(parameter?.ToString(), out bool isVisibleThenFalse);

            visible ^= !isVisibleThenFalse;
            return visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
