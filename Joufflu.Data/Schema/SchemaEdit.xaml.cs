using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Schema
{
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

    /// Logique d'interaction pour SchemaEdit.xaml
    /// </summary>
    public partial class SchemaEdit : UserControl
    {
        public SchemaObject Root { get; set; }

        public bool IsReadOnly { get; set; }

        public SchemaEdit()
        {
            Root = new SchemaObject();

            var sub = new SchemaObject();
            sub.Add("sub", new SchemaValue() { DataType = EnumDataType.String });

            Root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean });
            Root.Add("toto", new SchemaArray());
            Root.Add("titi", sub);

            InitializeComponent();
        }
    }
}