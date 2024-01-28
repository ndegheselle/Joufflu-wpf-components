using AdonisUI.Controls;
using System.Diagnostics;

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
    }
}
