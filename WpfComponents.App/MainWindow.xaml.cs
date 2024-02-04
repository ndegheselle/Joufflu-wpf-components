using AdonisUI.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;

namespace WpfComponents.App
{
    public class TestValue
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public TestValue(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return $"{Name} : {Value}";
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        public List<TestValue> TestValues { get; set; } = new List<TestValue>
        {
            new TestValue("One", "1"),
            new TestValue("Two", "2"),
            new TestValue("Three", "3"),
            new TestValue("Four", "4"),
            new TestValue("Five", "5"),
            new TestValue("Six", "6"),
            new TestValue("Seven", "7"),
            new TestValue("Eight", "8"),
            new TestValue("Nine", "9"),
            new TestValue("Ten", "10"),
        };

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
            Debug.WriteLine($"PhoneNumberInput.ValueChanged: {string.Join(',', values)}");
        }
        
    }
}
