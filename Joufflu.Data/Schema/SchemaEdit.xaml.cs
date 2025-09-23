using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Schema;

namespace Joufflu.Data.Schema
{
    #region Template selector
    public class SchemaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ParentTemplate { get; set; }

        public DataTemplate? ElementTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not SchemaProperty property)
                throw new InvalidOperationException($"The item must be of type '{typeof(SchemaProperty)}'.");

            return property.Element switch
            {
                _ when property.Element is ISchemaParent parent => ParentTemplate,
                _ when property.Element is ISchemaElement element => ElementTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
    #endregion

    /// Logique d'interaction pour SchemaEdit.xaml
    /// </summary>
    public partial class SchemaEdit : UserControl
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(nameof(Root), typeof(SchemaObject), typeof(SchemaEdit), new PropertyMetadata(null));

        public SchemaObject Root
        {
            get { return (SchemaObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public bool IsReadOnly { get; set; }

        public SchemaEdit()
        {
            InitializeComponent();
        }
    }
}