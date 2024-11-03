using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Data.Components
{
    /// <summary>
    /// Logique d'interaction pour Paging.xaml
    /// </summary>
    public partial class Paging : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public delegate void HandlePagingChange(int pageNumber, int capacity);

        public event HandlePagingChange? PagingChange;

        #region Dependency properties
        public int Total { get { return (int)GetValue(TotalProperty); } set { SetValue(TotalProperty, value); } }

        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(
            nameof(Total),
            typeof(int),
            typeof(Paging),
            new PropertyMetadata(0, (o, value) => ((Paging)o).OnTotalChanged()));

        private void OnTotalChanged()
        {
            OnPropertyChanged(nameof(PageMax));
            OnPropertyChanged(nameof(IntervalMin));
            OnPropertyChanged(nameof(IntervalMax));
        }

        // XXX : currently start a 1, start a 0 ?
        public int PageNumber
        {
            get { return (int)GetValue(PageNumberProperty); }
            set { SetValue(PageNumberProperty, value); }
        }

        public static readonly DependencyProperty PageNumberProperty =
            DependencyProperty.Register(
            nameof(PageNumber),
            typeof(int),
            typeof(Paging),
            new PropertyMetadata(1, (o, value) => ((Paging)o).OnPageNumberChange()));

        private void OnPageNumberChange()
        {
            int value = (int)GetValue(PageNumberProperty);
            if (value > PageMax)
                value = PageMax;
            if (value < 1)
                value = 1;

            SetValue(PageNumberProperty, value);

            PagingChange?.Invoke(PageNumber, Capacity);
            OnPropertyChanged(nameof(IntervalMin));
            OnPropertyChanged(nameof(IntervalMax));
        }

        public int Capacity
        {
            get { return (int)GetValue(CapacityProperty); }
            set { SetValue(CapacityProperty, value); }
        }

        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register(
            nameof(Capacity),
            typeof(int),
            typeof(Paging),
            new PropertyMetadata(10, (o, value) => ((Paging)o).OnCapacityChanged()));

        private void OnCapacityChanged()
        {
            if (PageNumber > PageMax && PageMax != 0)
                PageNumber = PageMax;

            PagingChange?.Invoke(PageNumber, Capacity);
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageMax));
            OnPropertyChanged(nameof(IntervalMin));
            OnPropertyChanged(nameof(IntervalMax));
        }
        #endregion

        #region Properties
        public List<int> AvailableCapacities { get; set; } = new List<int>() { 5, 10, 25, 50, 100, 200 };

        public int PageMax { 
            get {
                // Total is not set so we allow navigation to "unlimited" pages
                if (Total <= 0)
                    return int.MaxValue;
                int max = (int)Math.Ceiling(Total / (double)Capacity);
                return Math.Max(1, max);
            } 
        }

        public int IntervalMin { get { return Capacity * (PageNumber - 1) + 1; } }

        public int IntervalMax
        {
            get
            {
                if (IntervalMin + Capacity > Total)
                    return Total;
                else
                    return IntervalMin + Capacity - 1;
            }
        }
        #endregion

        public Paging() { InitializeComponent(); }

        #region Methodes
        public void Previous() { PageNumber -= 1; }

        public void Next() { PageNumber += 1; }

        public void First() { PageNumber = 1; }

        public void Last() { PageNumber = PageMax; }
        #endregion

        #region UI Events
        private void FirstPage_Click(object sender, RoutedEventArgs e) { First(); }

        private void PreviousPage_Click(object sender, RoutedEventArgs e) { Previous(); }

        private void NextPage_Click(object sender, RoutedEventArgs e) { Next(); }

        private void LastPage_Click(object sender, RoutedEventArgs e) { Last(); }


        private void TextBox_PreviewTextInput(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (PageMax > 1 && int.TryParse(textBox.Text, out int number))
            {
                int clamped = Math.Clamp(number, 1, PageMax);
                // Prevent infinite loop since PageNumber binded to TextBox.Text
                if (PageNumber != clamped)
                    PageNumber = clamped;
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
                Keyboard.ClearFocus();
        }
        #endregion
    }
}
