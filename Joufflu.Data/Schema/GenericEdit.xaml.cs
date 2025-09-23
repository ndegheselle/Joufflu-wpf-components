using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Usuel.Shared.Schema;

namespace Joufflu.Data.Schema
{
    #region Template selector
    public class GenericTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? ObjectKeyTemplate { get; set; }
        public DataTemplate? ArrayKeyTemplate { get; set; }
        public DataTemplate? NodeKeyTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                _ when item is PropertyWrapper prop && prop.Element is GenericObject => ObjectKeyTemplate,
                _ when item is PropertyWrapper prop && prop.Element is GenericArray => ArrayKeyTemplate,
                _ when item is PropertyWrapper prop && prop.Element is GenericValue => NodeKeyTemplate,
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

    #region Converters

    public class WrapValuesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is GenericArray array)
            {
                ObservableCollection<PropertyWrapper> wrappers = [];
                void AddElements(ObservableCollection<IGenericElement> _values, ObservableCollection<PropertyWrapper> _wrappers)
                {
                    for (int i = 0; i < _values.Count; i++)
                    {
                        _wrappers.Add(new PropertyWrapper((i + 1).ToString(), _values[i]));
                    }
                }

                AddElements(array.Values, wrappers);
                array.Values.CollectionChanged += (obj, args) =>
                {
                    wrappers.Clear();
                    AddElements(array.Values, wrappers);
                };
                return wrappers;
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
    #endregion

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
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(nameof(Root), typeof(GenericObject), typeof(GenericEdit), new PropertyMetadata(null));

        public GenericObject Root
        {
            get { return (GenericObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public GenericEdit()
        {
            InitializeComponent();
        }
    }
}
