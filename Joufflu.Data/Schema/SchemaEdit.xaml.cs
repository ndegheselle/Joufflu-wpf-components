using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.Data.Schema
{
    /// <summary>
    /// Convert a depth to a left margin for the schema properties.
    /// </summary>
    public class DepthToMarginConverter : IValueConverter
    {
        public object? Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is not uint depth)
                return null;
            return new Thickness(depth * 16, 0, 0, 0);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

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
        public bool IsReadOnly { get; set; }

        public SchemaEdit()
        {
            Root = new SchemaObject();

            var sub = new SchemaObject();
            sub.Add("sub", new SchemaValue() { DataType = EnumDataType.String }, 1);

            Root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean});
            Root.Add("toto", new SchemaValue() { DataType = EnumDataType.Decimal, IsArray = true});
            Root.Add("titi", sub);

            InitializeComponent();
        }
    }
}
