using AdonisUI.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Documents;

namespace WpfComponents.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TimePickerInput_ValueChanged(object sender, System.TimeSpan? e)
        {
            Debug.WriteLine($"TimePicker.ValueChanged: {e}");
        }

        private void PhoneNumberInput_ValueChanged(object sender, List<object?> values)
        {
            Debug.WriteLine($"CardInput.ValueChanged: {string.Join(',', values)}");
        }
        
    }
}
