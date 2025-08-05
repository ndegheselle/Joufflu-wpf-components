using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Samples
{
    /// <summary>
    /// Logique d'interaction pour InputsSamples.xaml
    /// </summary>
    public partial class InputsSamples : UserControl
    {
        public List<TestClass> TestValues
        {
            // Create a new instance because of CollectionViewSource.GetDefaultView
            get => new List<TestClass>
            {
                new TestClass("One", 1),
                new TestClass("Two", 2),
                new TestClass("Three", 3),
                new TestClass("Four", 4),
                new TestClass("Five", 5),
                new TestClass("Six", 6),
                new TestClass("Seven", 7),
                new TestClass("Eight", 8),
                new TestClass("Nine", 9),
                new TestClass("Ten", 10),
                new TestClass("Eleven", 11),
                new TestClass("Twelve", 12),
                new TestClass("Thirteen", 13),
                new TestClass("Fourteen", 14),
                new TestClass("Fifteen", 15),
                new TestClass("Sixteen", 16),
                new TestClass("Seventeen", 17),
                new TestClass("Eighteen", 18),
                new TestClass("Nineteen", 19),
                new TestClass("Twenty", 20),
                new TestClass("Twenty-One", 21),
            };
        }

        public List<string> TestStringValues
        {
            get => new List<string>
            {
                "One",
                "Two",
                "Three",
                "Four",
                "Five",
                "Six",
                "Seven",
                "Eight",
                "Nine",
                "Ten",
                "Eleven",
                "Twelve",
                "Thirteen",
                "Fourteen",
                "Fifteen",
                "Sixteen",
                "Seventeen",
                "Eighteen",
                "Nineteen",
                "Twenty",
                "Twenty-One"
            };
        }

        public TestClass TestValue { get; set; } = new TestClass("Minus", -1);

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
            TestValue.ClearErrors();
            if (string.IsNullOrEmpty(TestValue.FilePath))
                TestValue.AddError("No file selected", nameof(TestClass.FilePath));
        }

        private void SearchInput_SearchChanged(string text)
        {
            SearchValue.Text = text;
        }
    }
}
