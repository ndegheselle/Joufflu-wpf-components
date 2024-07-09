using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using WpfComponents.Lib.Components.Inputs.Format;

namespace WpfComponents.Lib.Components.Inputs
{
    // IValueConverter from TimeSpan to List<int> and back
    public class TimeSpanToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan date)
                return new List<object?>() { date.Days, date.Hours, date.Minutes, date.Seconds };
            else
                return new List<object?>() { null, null, null, null };
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = (List<object?>)value;
            if (list.Any(x => x == null))
                return null;

            return new TimeSpan((int)list[0], (int)list[1], (int)list[2], (int)list[3]);
        }
    }

    public partial class TimeSpanPicker : SingleValueFormatTextBox<TimeSpan?>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(TimeSpan?), typeof(TimeSpanPicker), new PropertyMetadata(default(TimeSpan?), (o, e) => ((TimeSpanPicker)o).OnValueChanged(e)
        ));

        public override TimeSpan? Value
        {
            get { return (TimeSpan?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public override TimeSpan? ConvertFrom()
        {
            if (Values.Any(x => x == null))
                return null;

            return new TimeSpan((int)Values[0], (int)Values[1], (int)Values[2], (int)Values[3]);
        }

        public override List<object?> ConvertTo()
        {
            if (Value is TimeSpan date)
                return new List<object?>() { date.Days, date.Hours, date.Minutes, date.Seconds };
            else
                return new List<object?>() { null, null, null, null };
        }
    }
}
