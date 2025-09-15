using System.Diagnostics;
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
            if (value == null)
                return null;

            if (value is not ISchemaElement)
                throw new InvalidOperationException("Can't get depth without a schema element.");
            ISubSchemaElement? schema = value as ISubSchemaElement;
            uint depth = 0;
            while(schema?.Parent != null)
            {
                schema = schema.Parent;
                depth++;
            }
            return new Thickness((depth - 1) * 16, 0, 0, 0);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
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
            sub.Add("sub", new SchemaValue() { DataType = EnumDataType.String });

            Root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean});
            Root.Add("toto", new SchemaValue() { DataType = EnumDataType.Decimal, IsArray = true});
            Root.Add("titi", sub);

            InitializeComponent();
        }
    }
}
