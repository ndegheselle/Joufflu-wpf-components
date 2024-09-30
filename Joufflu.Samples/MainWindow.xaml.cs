using AdonisUI.Controls;
using Joufflu.Popups;
using Joufflu.Shared;
using Joufflu.Shared.Layouts;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Usuel.Shared;

namespace Joufflu.Samples
{
    public class TestModal : TextBlock, IModalContent
    {
        public ModalOptions Options => new ModalOptions()
        {
            Title = "Test",
        };

        public TestModal()
        {
            Text = "Beep";
            MinWidth = 200;
            MinHeight = 100;
        }
    }

    public class TestModalValidation : TextBlock, IModalContentValidation
    {
        public ModalValidationOptions Options => new ModalValidationOptions()
        {
            Title = "What",
        };

        public TestModalValidation(string text)
        {
            Text = text;
            MinWidth = 200;
            MinHeight = 100;
        }

        public Task<bool> OnValidation()
        {
            return Task.FromResult(true);
        }
    }

    public enum EnumTest
    {
        None,
        One,
        Two,
    }

    public class TestClass : ErrorValidationModel
    {
        public string Name { get; set; }

        public int Value { get; set; }

        public bool IsTest { get; set; }

        public EnumTest EnumTest { get; set; }

        public string FilePath { get; set; } = "";

        public TestClass(string name, int value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString() { return $"{Name} : {Value}"; }

        public override bool Equals(object? obj) { return obj is TestClass value && Name == value.Name; }

        public override int GetHashCode() { return Name.GetHashCode(); }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow, INotifyPropertyChanged
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

        public MainWindow()
        {
            InitializeComponent();
            // Foreach tab in MainContainer create a ListBoxItem and add it to the ListBox
            foreach (TabItem tab in MainContainer.Items)
            {
                var item = new ListBoxItem { Content = tab.Header, Tag = tab };
                item.Selected += (s, e) =>
                {
                    MainContainer.SelectedItem = (TabItem)((ListBoxItem)s).Tag;
                };
                SideMenu.Items.Add(item);
            }

            PagingDataGrid.ItemsSource = TestValues.Take(5);
        }

        #region Inputs

        private void PhoneNumberInput_ValueChanged(object sender, List<object?> values)
        { Debug.WriteLine($"PhoneNumberInput.ValueChanged: {string.Join(',', values)}"); }

        private void NumericUpDownInput_ValueChanged(object sender, int e)
        {
            Debug.WriteLine($"NumerUpDown.ValueChanged: {e}");
        }

        private void DecimalUpDownInput_ValueChanged(object sender, decimal e)
        {
            Debug.WriteLine($"DecimalUpDown.ValueChanged: {e}");
        }

        private void TimePickerInput_ValueChanged(object sender, TimeSpan? e)
        {
            Debug.WriteLine($"TimePicker.ValueChanged: {e}");
        }

        private void ButtonCheckFile_Click(object sender, RoutedEventArgs e)
        {
            TestValue.ClearErrors();
            if (string.IsNullOrEmpty(TestValue.FilePath))
                TestValue.AddError("No file selected", nameof(TestClass.FilePath));
        }
        #endregion

        #region Data

        private void Paging_PagingChange(int pageNumber, int capacity)
        {
            PagingDataGrid.ItemsSource = TestValues.Skip((pageNumber - 1) * capacity).Take(capacity);
        }

        #endregion

        #region Popups

        private void ShowAlert_Click(object sender, RoutedEventArgs e)
        {
            Alert.Success("Success alert.");
        }

        private async void ShowModal_Click(object sender, RoutedEventArgs e)
        {
            await Modal.Show(new TestModal());
        }

        private async void ShowModalValidation_Click(object sender, RoutedEventArgs e)
        {
            bool result = await Modal.Show(new TestModalValidation("what"));
        }

        private async void ShowMultiple_Click(object sender, RoutedEventArgs e)
        {

            Modal.Show(new TestModalValidation("boop"));
            Modal.Show(new TestModalValidation("beep"));
        }
        #endregion

    }
}