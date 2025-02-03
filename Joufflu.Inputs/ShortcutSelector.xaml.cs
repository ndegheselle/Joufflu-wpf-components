using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Inputs
{
    /// <summary>
    /// Logique d'interaction pour KeyboardShortcutSelector.xaml
    /// </summary>
    public partial class ShortcutSelector : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        #region Dependency properties

        public static readonly DependencyProperty ShortcutProperty = DependencyProperty.Register(
            nameof(Shortcut), typeof(ObservableCollection<Key>), typeof(ShortcutSelector), new FrameworkPropertyMetadata(new ObservableCollection<Key>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ObservableCollection<Key> Shortcut
        {
            get => (ObservableCollection<Key>)GetValue(ShortcutProperty);
            set => SetValue(ShortcutProperty, value);
        }

        #endregion

        public bool IsEditing { get; set; } = false;
        private readonly int MaxNumberOfKeys = 3;

        public ShortcutSelector()
        {
            InitializeComponent();
        }

        #region Methods

        private void ReorderShortcut()
        {
            var sortedShortcut = Shortcut.OrderByDescending(x => x).ToList();
            Shortcut.Clear();
            foreach (var key in sortedShortcut)
            {
                Shortcut.Add(key);
            }
        }

        #endregion

        #region UI events
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            Shortcut.Clear();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            IsEditing = true;
        }

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            IsEditing = false;
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsEditing == false) return;

            if (Shortcut.Count >= MaxNumberOfKeys || e.Key == Key.Escape)
            {
                IsEditing = false;
                return;
            }

            if (Shortcut.Contains(e.Key)) return;

            Shortcut.Add(e.Key);
            ReorderShortcut();
        }

        #endregion
    }
}
