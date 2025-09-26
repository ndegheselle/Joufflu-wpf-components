using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Schema;

namespace Joufflu.Data.Schema
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
                _ when item is GenericProperty prop && prop.Element is IGenericElement => ElementTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }

    public class ValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }
        public DataTemplate? DecimalTemplate { get; set; }
        public DataTemplate? BooleanTemplate { get; set; }
        public DataTemplate? DateTimeTemplate { get; set; }
        public DataTemplate? TimeSpanTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not GenericValue value)
                throw new InvalidOperationException($"The item must be of type '{typeof(GenericValue)}'.");
            
            return value.DataType switch
            {
                EnumDataType.String => StringTemplate,
                EnumDataType.Decimal => DecimalTemplate,
                EnumDataType.Boolean => BooleanTemplate,
                EnumDataType.DateTime => DateTimeTemplate,
                EnumDataType.TimeSpan => TimeSpanTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }

    #endregion

    /// <summary>
    /// Logique d'interaction pour GenericEdit.xaml
    /// </summary>
    public partial class GenericEdit : UserControl
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(nameof(Root), typeof(GenericObject), typeof(GenericEdit), new PropertyMetadata(null));

        public GenericObject Root
        {
            get { return (GenericObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public bool IsReadOnly { get; set; }
        public bool WithSchemaEdit { get; set; }
        public bool WithValueEdit { get; set; }

        public GenericEdit()
        {
            Root = new GenericObject();

            Root.CreateProperty("tata", EnumDataType.String);
            Root.CreateProperty("toto", EnumDataType.Array);
            Root.CreateProperty("titi", EnumDataType.Object);

            InitializeComponent();
        }

        private void EditIdentifierClick(object sender, RoutedEventArgs e)
        {
            EditIdentifierPopup.Show((FrameworkElement)((FrameworkElement)sender).Parent);
        }
    }
}