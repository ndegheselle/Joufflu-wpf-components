using Joufflu.Popups;
using System.Windows;
using System.Windows.Controls;
using Usuel.Shared;

namespace Joufflu.Samples.Views
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
    /// Logique d'interaction pour PopupsSamples.xaml
    /// </summary>
    public partial class PopupsSamples : UserControl
    {
        private IAlert alert => ((MainWindow)Application.Current.MainWindow).Alert;
        private ILoading loading => ((MainWindow)Application.Current.MainWindow).Loading;
        private IModal modal => ((MainWindow)Application.Current.MainWindow).Modal;

        public PopupsSamples()
        {
            InitializeComponent();
        }

        #region UI events
        private void ShowAlert_Click(object sender, RoutedEventArgs e)
        {
            alert.Success("Success alert.");
        }

        private async void ShowModal_Click(object sender, RoutedEventArgs e)
        {
            await modal.Show(new TestModal());
        }

        private void ShowMultiple_Click(object sender, RoutedEventArgs e)
        {
            modal.Show(new TestModalValidation("boop"));
            modal.Show(new TestModalValidation("beep"));
        }

        private async void ShowLoading_Click(object sender, RoutedEventArgs e)
        {
            loading.Show("Loading for 2s ...");
            await Task.Delay(2000);
            loading.Hide();
        }
        #endregion
    }
}
