using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Proto.Data
{
    #region Template selector
    public class GenericTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ParentTemplate { get; set; }
        public DataTemplate? ElementTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                _ when item is GenericProperty prop && prop.Element is IGenericParent => ParentTemplate,
                _ when item is GenericProperty prop && prop.Element is GenericElement => ElementTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }

    #endregion

    /// <summary>
    /// Logique d'interaction pour GenericEdit.xaml
    /// </summary>
    public partial class SchemaEdit : UserControl
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(nameof(Root), typeof(GenericObject), typeof(SchemaEdit), new PropertyMetadata(null));

        public GenericObject Root
        {
            get { return (GenericObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public bool IsReadOnly { get; set; }

        public SchemaEdit()
        {
            InitializeComponent();
        }

        private void EditIdentifierClick(object sender, RoutedEventArgs e)
        {
            EditIdentifierPopup.Show((FrameworkElement)((FrameworkElement)sender).Parent);
        }
    }
}