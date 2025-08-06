using AdonisUI.Controls;
using Joufflu.Popups;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Usuel.Shared;

namespace Joufflu.Samples
{
    public class TestModal : TextBlock, IModalContent
    {
        public IModal? ParentLayout { get; set; }
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

    public class TestModalValidation : TextBlock, IModalContent
    {
        public IModal? ParentLayout { get; set; }
        public ModalOptions Options { get; } = new ModalOptions()
        {
            Title = "What",
        };
        public ICustomCommand ValidateCommand { get; protected set; }

        public TestModalValidation(string text)
        {
            Text = text;
            MinWidth = 200;
            MinHeight = 100;
            ValidateCommand = new DelegateCommand(Validate);
            // Use ParentLayout?.HideCommand() or ParentLayout?.Hide() to close the modal
        }

        public void Validate()
        {
            // Do validation work
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow, INotifyPropertyChanged
    {
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

            PagingDataGrid.ItemsSource = Tests.Values.Take(5);
        }

        #region Data

        private void Paging_PagingChange(int pageNumber, int capacity)
        {
            PagingDataGrid.ItemsSource = Tests.Values.Skip((pageNumber - 1) * capacity).Take(capacity);
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

        private void ShowMultiple_Click(object sender, RoutedEventArgs e)
        {
            Modal.Show(new TestModalValidation("boop"));
            Modal.Show(new TestModalValidation("beep"));
        }
        #endregion
    }
}