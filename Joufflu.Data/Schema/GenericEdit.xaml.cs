using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Usuel.Shared;
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
                _ when item is GenericProperty prop && prop.Element is GenericObject => ObjectKeyTemplate,
                _ when item is GenericProperty prop && prop.Element is GenericArray => ArrayKeyTemplate,
                _ when item is GenericProperty prop && prop.Element is GenericValue => NodeKeyTemplate,
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
            
            return value.DataType switch
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

    public class GenericProperty
    {
        public object Identifier { get; set; }
        public IGenericElement Element { get; }
        public bool IsConst { get; set; } = false;
        public ICustomCommand RemoveCommand { get; }

        public GenericProperty(object identifier, IGenericElement element)
        {
            Identifier = identifier;
            Element = element;
            RemoveCommand = new DelegateCommand(() => Element.Parent?.Remove(this), () => IsConst == false);
        }
    }

    public class WrapValuesConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value switch
            {
                _ when value is GenericObject obj => obj.Properties.Select(x => new GenericProperty(x.Key, x.Value)),
                _ when value is GenericArray array => [
                    new GenericProperty("Schema", array.Schema) { IsConst = true },
                    ..array.Values.Select((val, index) => new GenericProperty(index, val))
                ],
                _ => null
            };
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

        public bool IsReadOnly { get; set; }
        public bool WithSchemaEdit { get; set; }
        public bool WithValueEdit { get; set; }

        public GenericEdit()
        {
            InitializeComponent();
        }
    }
}
