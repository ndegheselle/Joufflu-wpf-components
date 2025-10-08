using NJsonSchema;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Json
{
    public class PropertyTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ObjectTemplate { get; set; }
        public DataTemplate? ArrayTemplate { get; set; }
        public DataTemplate? ValueTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            JsonSchemaProperty property = (JsonSchemaProperty)item;
            return property.Type switch
            {
                JsonObjectType.Object => ObjectTemplate,
                JsonObjectType.Array => ArrayTemplate,
                _ => ValueTemplate
            };
        }
    }

    /// <summary>
    /// Logique d'interaction pour JsonEditor.xaml
    /// </summary>
    public partial class JsonEditor : UserControl
    {
        public static readonly DependencyProperty SchemaProperty =
            DependencyProperty.Register(nameof(Schema), typeof(JsonSchema), typeof(JsonEditor), new PropertyMetadata(null));
        public JsonSchema Schema { get { return (JsonSchema)GetValue(SchemaProperty); } set { SetValue(SchemaProperty, value); } }

        public JsonEditor() {
            InitializeComponent();
        }
    }
}
