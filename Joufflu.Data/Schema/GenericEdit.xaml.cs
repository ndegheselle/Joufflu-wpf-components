using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Schema
{
    public class GenericTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ObjectTemplate { get; set; }
        public DataTemplate? ArrayTemplate { get; set; }
        public DataTemplate? ValueTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not IGenericNode node)
                throw new InvalidOperationException($"The item must be of type '{typeof(IGenericNode)}'.");

            if (node.Schema is SchemaObject schemaObject)
            {
                if (schemaObject.IsArray)
                    return ArrayTemplate;
                return ObjectTemplate;
            }
            else if (node.Schema is SchemaValue schemaValue)
            {
                if (schemaValue.IsArray)
                    return ArrayTemplate;
                return ValueTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }

    /// <summary>
    /// Logique d'interaction pour GenericEdit.xaml
    /// </summary>
    public partial class GenericEdit : UserControl
    {
        public GenericObject Root { get; set; }
        public GenericEdit()
        {
            var root = new SchemaObject();

            var sub = new SchemaObject();
            sub.Add("sub", new SchemaValue() { DataType = EnumDataType.String });

            root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean });
            root.Add("toto", new SchemaValue() { DataType = EnumDataType.Decimal, IsArray = true });
            root.Add("titi", sub);

            Root = (GenericObject)root.ToValue();

            InitializeComponent();
        }
    }
}
