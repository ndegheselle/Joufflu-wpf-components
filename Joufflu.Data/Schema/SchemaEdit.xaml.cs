using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Schema
{
    public class SchemaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ObjectTemplate { get; set; }
        public DataTemplate? ValueTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not SchemaProperty property)
                throw new InvalidOperationException($"The item must be of type '{typeof(SchemaProperty)}'.");

            if (property.Element is SchemaValue)
            {
                return ValueTemplate;
            }
            else if (property.Element is SchemaObject)
            {
                return ObjectTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }

    /// <summary>
    /// Logique d'interaction pour SchemaEdit.xaml
    /// </summary>
    public partial class SchemaEdit : UserControl
    {
        public SchemaObject Root { get; set; }

        public SchemaEdit()
        {
            Root = new SchemaObject();

            var sub = new SchemaObject();
            sub.Properties.Add(new SchemaProperty("sub", new SchemaValue() { DataType = EnumDataType.String }));

            Root.Properties.Add(new SchemaProperty("tata", new SchemaValue() { DataType = EnumDataType.Boolean}));
            Root.Properties.Add(new SchemaProperty("toto", new SchemaValue() { DataType = EnumDataType.Decimal, IsArray = true}));
            Root.Properties.Add(new SchemaProperty("titi", sub));

            InitializeComponent();
        }
    }
}
