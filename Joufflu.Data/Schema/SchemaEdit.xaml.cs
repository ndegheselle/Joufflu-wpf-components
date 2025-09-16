using System.Collections;
using System.Globalization;
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
            CultureInfo culture)
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
            CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class ObjectToListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable enumerable && !(value is string))
                return enumerable;

            if (value != null)
                return new List<object> { value };

            return new List<object>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
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
            sub.Add("sub", new SchemaValue() { DataType = EnumDataType.String });

            Root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean});
            Root.Add("toto", new SchemaArray());
            Root.Add("titi", sub);

            InitializeComponent();
        }
    }
}
