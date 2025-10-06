using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Joufflu.Proto.Data.Components
{
    /// <summary>
    /// Logique d'interaction pour EditIdentifierPopup.xaml
    /// </summary>
    public partial class EditIdentifierPopup : Popup, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public GenericProperty? Property { get; private set; }
        public string Identifier { get; set; } = "";
        public string MessageError { get; set; } = "";

        public EditIdentifierPopup()
        {
            InitializeComponent();
        }

        public void Show(FrameworkElement element)
        {
            var property = element.DataContext as GenericProperty;

            if (property == null)
                throw new Exception($"The target element should have a '{nameof(GenericProperty)}' type.");

            MessageError = "";
            Property = property;
            Identifier = Property.Identifier.ToString()!;
            PlacementTarget = element;
            IsOpen = true;
        }

        private void Button_Cancel(object sender, System.Windows.RoutedEventArgs e)
        {
            this.IsOpen = false;
        }

        private void Button_Validate(object sender, System.Windows.RoutedEventArgs e)
        {
            if (Property == null)
                return;

            if (Property.Identifier.ToString()! == Identifier)
            {
                this.IsOpen = false;
                return;
            }
                

            // Check if unique
            if (Property.Element.Parent?.ChangeIdentifier(Property.Identifier, Identifier) == false)
            {
                MessageError = $"'{Identifier}' is already used.";
                return;
            }

            Property.Identifier = Identifier;
            this.IsOpen = false;
        }
    }
}
