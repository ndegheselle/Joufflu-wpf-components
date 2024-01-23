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
    // IValueConverter from DateTime to List<int> and back
    public class DateTimeToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            return new List<object>() { date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var list = value as List<object>;
            return new DateTime((int)list[0], (int)list[1], (int)list[2], (int)list[3], (int)list[4], (int)list[5]);
        }
    }

    // XXX : use classic string format, only have to find a way to link the string format to the actual DateTime. or maybe using regex and group ?
    public partial class TimePicker : UserControl, INotifyPropertyChanged
    {
        public DateTime TestDate { get; set; } = DateTime.Now;

        public TimePicker() { InitializeComponent(); }
    }
}
