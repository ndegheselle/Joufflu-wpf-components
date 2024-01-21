using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WpfComponents.Lib.Inputs.Formated
{
    // Converter DateTime to a list of string and vice versa

    public class DateTimePartsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime = (DateTime)value;

            return new List<string>()
            {
                dateTime.Year.ToString(),
                dateTime.Month.ToString(),
                dateTime.Day.ToString(),
                dateTime.Hour.ToString(),
                dateTime.Minute.ToString(),
                dateTime.Second.ToString(),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> parts = (List<string>)value;

            return new DateTime(
                int.Parse(parts[0]),
                int.Parse(parts[1]),
                int.Parse(parts[2]),
                int.Parse(parts[3]),
                int.Parse(parts[4]),
                int.Parse(parts[5]));
        }
    }
}
