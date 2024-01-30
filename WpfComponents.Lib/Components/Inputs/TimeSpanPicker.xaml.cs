using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WpfComponents.Lib.Helpers;

namespace WpfComponents.Lib.Inputs
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = (List<object?>)value;
            if (list.Any(x => x == null))
                return null;

            return new TimeSpan((int)list[0], (int)list[1], (int)list[2], (int)list[3]);
        }
    }

    public partial class TimeSpanPicker : UserControl, INotifyPropertyChanged
    {
        public event EventHandler<TimeSpan?>? ValueChanged;

        // TODO : should be a DP
        private TimeSpan? _previousValue;
        public TimeSpan? Value { get; set; }
        public TimeSpanPicker() { InitializeComponent(); }

        private void FormatedTextBox_ValuesChanged(object sender, List<object> e)
        {
            if (Value == _previousValue)
                return;

            ValueChanged?.Invoke(this, Value);
            _previousValue = Value;
        }
    }
}
