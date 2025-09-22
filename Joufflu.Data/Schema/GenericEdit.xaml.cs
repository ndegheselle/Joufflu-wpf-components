using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.Data.Schema
{
    #region Template selector
    public class GenericTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ObjectKeyTemplate { get; set; }
        public DataTemplate? ArrayKeyTemplate { get; set; }
        public DataTemplate? NodeKeyTemplate { get; set; }
        public DataTemplate? ObjectTemplate { get; set; }
        public DataTemplate? ArrayTemplate { get; set; }
        public DataTemplate? NodeTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                _ when item is GenericObject => ObjectTemplate,
                _ when item is GenericArray => ArrayTemplate,
                _ when item is GenericValue => NodeTemplate,
                _ when item is KeyValuePair<string, IGenericElement> keyValue && keyValue.Value is GenericObject => ObjectKeyTemplate,
                _ when item is KeyValuePair<string, IGenericElement> keyValue && keyValue.Value is GenericArray => ArrayKeyTemplate,
                _ when item is KeyValuePair<string, IGenericElement> keyValue && keyValue.Value is GenericValue => NodeKeyTemplate,
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
            
            return value.Schema?.DataType switch
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

    public class WrapValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is GenericArray array)
            {
                ObservableCollection<PropertyWrapper> wrappers = [];
                for(int i = 0; i <= array.Values.Count; i++)
                {
                    wrappers.Add(new PropertyWrapper(i.ToString(), array.Values[i]));
                }

                array.Values.CollectionChanged += (obj, args) =>
                {
                    wrappers.Clear();
                    for (int i = 0; i <= array.Values.Count; i++)
                    {
                        wrappers.Add(new PropertyWrapper(i.ToString(), array.Values[i]));
                    }
                };
            }
            else if (value is GenericObject obj)
            {
                return obj.Properties.Select(x => new PropertyWrapper(x.Key, x.Value));
            }
            return new List<PropertyWrapper>();
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue;
            return value;
        }
    }

    public class PropertyWrapper
    {
        public string Name { get; }
        public IGenericElement Element { get; }

        public PropertyWrapper(string name, IGenericElement element)
        {
            Name = name;
            Element = element;
        }

        public override bool Equals(object? obj)
        {
            return Element.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Element.GetHashCode();
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

            var subArray = new SchemaObject();
            subArray.Add("sub", new SchemaValue() { DataType = EnumDataType.String });

            root.Add("tata", new SchemaValue() { DataType = EnumDataType.Boolean });
            root.Add("toto", new SchemaArray(subArray) );
            root.Add("titi", sub);

            Root = (GenericObject)root.ToValue();

            InitializeComponent();
        }
    }
}
