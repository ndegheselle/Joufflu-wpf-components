using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Usuel.Shared;

namespace Joufflu.Data
{
    /// <summary>
    /// TODO : Use command and a control template
    /// </summary>
    public partial class Paging : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public delegate void HandlePagingChange(int pageNumber, int capacity);
        public event HandlePagingChange? PagingChange;

        private TextBox? _inputPage;

        #region DependencyProperties
        public static readonly DependencyProperty TotalProperty =
            DependencyProperty.Register(
                nameof(Total),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(0, (o, value) => ((Paging)o).OnTotalChanged()));

        public static readonly DependencyProperty PageNumberProperty =
            DependencyProperty.Register(
                nameof(PageNumber),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(1, (o, value) => ((Paging)o).OnPageNumberChange()));

        public static readonly DependencyProperty CapacityProperty =
            DependencyProperty.Register(
                nameof(Capacity),
                typeof(int),
                typeof(Paging),
                new PropertyMetadata(10, (o, value) => ((Paging)o).OnCapacityChanged()));
        #endregion

        #region Properties
        public int Total { get { return (int)GetValue(TotalProperty); } set { SetValue(TotalProperty, value); } }
        public int PageNumber { get { return (int)GetValue(PageNumberProperty); } set { SetValue(PageNumberProperty, value); } }
        public int Capacity { get { return (int)GetValue(CapacityProperty); } set { SetValue(CapacityProperty, value); } }

        public List<int> AvailableCapacities { get; set; } = new List<int>() { 5, 10, 25, 50, 100, 200 };

        public int PageMax
        {
            get
            {
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

        #region Commands
        public ICustomCommand PreviousCommand { get; private set; }
        public ICustomCommand NextCommand { get; private set; }
        public ICustomCommand FirstCommand { get; private set; }
        public ICustomCommand LastCommand { get; private set; }
        #endregion

        public Paging()
        {
            PreviousCommand = new DelegateCommand(Previous, () => PageNumber > 1);
            NextCommand = new DelegateCommand(Next, () => PageNumber < PageMax);
            FirstCommand = new DelegateCommand(First, () => PageNumber > 1);
            LastCommand = new DelegateCommand(Last, () => PageNumber < PageMax);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _inputPage = GetTemplateChild("PART_InputPage") as TextBox;
            if (_inputPage != null)
            {
                _inputPage.PreviewTextInput += TextBox_PreviewTextInput;
                _inputPage.KeyDown += TextBox_OnKeyDown;
            }
        }

        #region Change Events
        private void OnTotalChanged()
        {
            OnPropertyChanged(nameof(PageMax));
            OnPropertyChanged(nameof(IntervalMin));
            OnPropertyChanged(nameof(IntervalMax));
            RaiseCommandsChanged();
        }

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
            RaiseCommandsChanged();
        }

        private void OnCapacityChanged()
        {
            if (PageNumber > PageMax && PageMax != 0)
                PageNumber = PageMax;

            PagingChange?.Invoke(PageNumber, Capacity);
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageMax));
            OnPropertyChanged(nameof(IntervalMin));
            OnPropertyChanged(nameof(IntervalMax));
            RaiseCommandsChanged();
        }

        private void RaiseCommandsChanged()
        {
            PreviousCommand.RaiseCanExecuteChanged();
            NextCommand.RaiseCanExecuteChanged();
            FirstCommand.RaiseCanExecuteChanged();
            LastCommand.RaiseCanExecuteChanged();
        }
        #endregion

        #region Methods
        public void Previous() { PageNumber -= 1; }
        public void Next() { PageNumber += 1; }
        public void First() { PageNumber = 1; }
        public void Last() { PageNumber = PageMax; }
        #endregion

        #region UI Events
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (_inputPage == null) return;
            if (PageMax > 1 && int.TryParse(_inputPage.Text, out int number))
            {
                int clamped = Math.Clamp(number, 1, PageMax);
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