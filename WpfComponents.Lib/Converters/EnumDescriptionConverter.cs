using System;
using System.Windows.Data;
using WpfComponents.Lib.Logic.Helpers;

namespace WpfComponents.Lib.Converters
{
    internal class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var enumTarget = value as Enum;
            if (enumTarget == null)
                return null;

            return enumTarget.GetDescription();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
