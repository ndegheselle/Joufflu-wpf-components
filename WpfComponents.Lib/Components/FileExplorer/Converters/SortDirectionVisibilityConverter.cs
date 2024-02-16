using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;

namespace WpfComponents.Lib.Components.FileExplorer.Converters
{
    class SortDirectionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null) return Visibility.Collapsed;

            ListSortDirection valueDirection = (ListSortDirection)value;
            ListSortDirection targetDirection = (ListSortDirection)Enum.Parse(typeof(ListSortDirection), parameter.ToString());

            return valueDirection == targetDirection ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
