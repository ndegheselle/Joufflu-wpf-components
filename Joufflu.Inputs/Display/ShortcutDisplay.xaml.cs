using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Joufflu.Inputs.Display
{
    // Dunno why System.Windows.Input.Key is not a valid binding :I
    public class KeyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Logique d'interaction pour KeyboardShortcut.xaml
    /// </summary>
    public partial class ShortcutDisplay : UserControl
    {
        #region Dependency properties

        public static readonly DependencyProperty ShortcutProperty = DependencyProperty.Register(
            "Shortcut", typeof(IEnumerable<Key>), typeof(ShortcutDisplay), new PropertyMetadata(new List<Key>() { Key.LeftShift, Key.A }));

        public IEnumerable<Key> Shortcut
        {
            get => (IEnumerable<Key>)GetValue(ShortcutProperty);
            set => SetValue(ShortcutProperty, value);
        }

        #endregion

        public ShortcutDisplay()
        {
            InitializeComponent();
        }
    }
}
