using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Samples.Views
{
    /// <summary>
    /// Logique d'interaction pour InputsSamples.xaml
    /// </summary>
    public partial class InputsSamples : UserControl
    {
        public List<TestClass> ComboBoxSearchValues { get; private set; } = Tests.Values;
        public List<TestClass> ComboBoxTagsValues { get; private set; } = Tests.Values;

        public ObservableCollection<TestClass> SelectedTestValues
        {
            get;
            set;
        } = new ObservableCollection<TestClass>
        {
            new TestClass("One", 1),
            new TestClass("Two", 2),
            new TestClass("Three", 3),
        };

        public InputsSamples() { InitializeComponent(); }

        private void PhoneNumberInput_ValueChanged(object sender, List<object?> values)
        { Debug.WriteLine($"PhoneNumberInput.ValueChanged: {string.Join(',', values)}"); }

        private void NumericUpDownInput_ValueChanged(object sender, int e)
        { Debug.WriteLine($"NumerUpDown.ValueChanged: {e}"); }

        private void DecimalUpDownInput_ValueChanged(object sender, decimal e)
        { Debug.WriteLine($"DecimalUpDown.ValueChanged: {e}"); }

        private void TimePickerInput_ValueChanged(object sender, TimeSpan? e)
        { Debug.WriteLine($"TimePicker.ValueChanged: {e}"); }

        private void ButtonCheckFile_Click(object sender, RoutedEventArgs e)
        {
            Tests.Value.ClearErrors();
            if (string.IsNullOrEmpty(Tests.Value.FilePath))
                Tests.Value.AddError("No file selected", nameof(TestClass.FilePath));
        }

        private void SearchInput_SearchChanged(string text)
        {
            SearchValue.Text = text;
        }
    }
}
