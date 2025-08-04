using System.Windows.Controls;
using System.Windows.Threading;
using Usuel.Shared;

namespace Joufflu.Inputs
{
    /// <summary>
    /// Search input with build in delay to limit the number of requests to an API or database.
    /// </summary>
    public partial class Search : TextBox
    {
        public event Action<string>? SearchChanged;
        public DelegateCommand ClearCommand { get; set; }

        private readonly DispatcherTimer _searchTimer;
        public Search()
        {
            _searchTimer = InitSearchTimer();
            ClearCommand = new DelegateCommand(Clear);
            this.KeyUp += OnKeyUp;
        }

        private DispatcherTimer InitSearchTimer()
        {
            var lFiltreTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            lFiltreTimer.Tick += FilterTimer_Tick;
            return lFiltreTimer;
        }

        private void OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                Clear();
                return;
            }

            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void FilterTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            SearchChanged?.Invoke(this.Text);
        }

        public void Clear()
        {
            this.Text = "";
            SearchChanged?.Invoke(this.Text);
        }
    }
}
