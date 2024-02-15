using AdonisUI.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
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

        public override bool Equals(object? obj)
        {
            return obj is TestValue value &&
                   Name == value.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow, INotifyPropertyChanged
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

        public ObservableCollection<TestValue> SelectedTestValues { get; set; } = new ObservableCollection<TestValue>
        {
            new TestValue("One", "1"),
            new TestValue("Two", "2"),
            new TestValue("Three", "3"),
        };

        public MainWindow()
        {
            InitializeComponent();
            // Foreach tab in MainContainer create a ListBoxItem and add it to the ListBox
            foreach (TabItem tab in MainContainer.Items)
            {
                var item = new ListBoxItem
                {
                    Content = tab.Header,
                    Tag = tab
                };
                item.Selected += (s, e) =>
                {
                    MainContainer.SelectedItem = (TabItem)((ListBoxItem)s).Tag;
                };
                SideMenu.Items.Add(item);
            }

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
