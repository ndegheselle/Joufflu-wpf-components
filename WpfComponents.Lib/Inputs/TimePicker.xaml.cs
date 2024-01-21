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
using System.Windows.Input;
using WpfComponents.Lib.Helpers;

namespace WpfComponents.Lib.Inputs
{
    // XXX : use classic string format, only have to find a way to link the string format to the actual DateTime. or maybe using regex and group ?
    public partial class TimePicker : UserControl, INotifyPropertyChanged
    {
        public DateTime TestDate { get; set; } = DateTime.Now;

        public TimePicker() { InitializeComponent(); }
    }
}
