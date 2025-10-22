using System.ComponentModel;
using System.Windows.Controls;
using Usuel.History;

namespace Joufflu.Samples.Views
{
    /// <summary>
    /// Logique d'interaction pour HistorySample.xaml
    /// </summary>
    public partial class HistorySample : UserControl, INotifyPropertyChanged
    {
        public HistoryHandler Handler { get; }

        public string TestText { get; set; } = "";
        private static readonly Random Random = new Random();

        public IReversibleCommand AddRandomCharCommand { get; }
        public IReversibleCommand RemoveLastCharCommand { get; }
        public IReversibleCommand AddMultipleRandomCharCommand { get; }
        public IReversibleCommand RemoveMultipleLastCharCommand { get; }

        public HistorySample()
        {
            Handler = new HistoryHandler();

            AddRandomCharCommand = new ReversibleCommand(Handler, AddRandomChar, name: "AddRandomChar");
            RemoveLastCharCommand = new ReversibleCommand(Handler, RemoveLastChar, name: "RemoveLastChar");
            AddMultipleRandomCharCommand = new ReversibleCommand(Handler, AddMultipleRandomChar, name: "AddMultipleRandomChar");
            RemoveMultipleLastCharCommand = new ReversibleCommand(Handler, RemoveMultipleLastChar, name: "RemoveMultipleLastChar");

            Handler.SetReverse(AddRandomCharCommand, RemoveLastCharCommand);
            Handler.SetReverse(AddMultipleRandomCharCommand, RemoveMultipleLastCharCommand);

            InitializeComponent();
        }

        private void AddRandomChar()
        {
            // Generate a random character between 'A' and 'Z'
            char randomChar = (char)Random.Next('A', 'Z' + 1);
            TestText += randomChar;
        }

        private void RemoveLastChar()
        {
            if (TestText.Length > 0)
            {
                TestText = TestText.Remove(TestText.Length - 1);
            }
        }

        private void AddMultipleRandomChar()
        {
            for (int i = 0; i < 10; i++)
            {
                AddRandomChar();
            }
        }

        private void RemoveMultipleLastChar()
        {
            for (int i = 0; i < 10; i++)
            {
                RemoveLastChar();
            }
        }
    }
}
