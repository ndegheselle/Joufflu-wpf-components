using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.Proto.Data
{
    #region Template selector
    public class ValueTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? StringTemplate { get; set; }

        public DataTemplate? IntegerTemplate { get; set; }

        public DataTemplate? DecimalTemplate { get; set; }

        public DataTemplate? BooleanTemplate { get; set; }

        public DataTemplate? DateTimeTemplate { get; set; }

        public DataTemplate? TimeSpanTemplate { get; set; }

        public DataTemplate? EnumTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not GenericValue value)
                throw new InvalidOperationException($"The item must be of type '{typeof(GenericValue)}'.");

            return value.DataType switch
            {
                EnumDataType.String => StringTemplate,
                EnumDataType.Integer => IntegerTemplate,
                EnumDataType.Decimal => DecimalTemplate,
                EnumDataType.Boolean => BooleanTemplate,
                EnumDataType.DateTime => DateTimeTemplate,
                EnumDataType.TimeSpan => TimeSpanTemplate,
                EnumDataType.Enum => EnumTemplate,
                _ => base.SelectTemplate(item, container)
            };
        }
    }
    #endregion

    public class GenericReferencesConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return Binding.DoNothing;

            EnumDataType datatype = (EnumDataType)values[0];
            Dictionary<EnumDataType, List<GenericReference>> availableReferencesPerType = (Dictionary<EnumDataType, List<GenericReference>>)values[
                1];

            // XXX : maybe types like string should accept other types references
            return availableReferencesPerType[datatype];
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Logique d'interaction pour GenericEdit.xaml
    /// </summary>
    public partial class ValueEdit : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(
            nameof(Root),
            typeof(GenericObject),
            typeof(ValueEdit),
            new PropertyMetadata(null));

        public static readonly DependencyProperty AvailableReferencesProperty =
            DependencyProperty.Register(
            nameof(AvailableReferences),
            typeof(IEnumerable<GenericReference>),
            typeof(ValueEdit),
            new PropertyMetadata(null, (o, e) => ((ValueEdit)o).OnAvailableReferencesChange()));

        public GenericObject Root
        {
            get { return (GenericObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public IEnumerable<GenericReference> AvailableReferences
        {
            get { return (IEnumerable<GenericReference>)GetValue(AvailableReferencesProperty); }
            set { SetValue(AvailableReferencesProperty, value); }
        }

        public bool IsReadOnly { get; set; }

        public Dictionary<EnumDataType, List<GenericReference>> AvailableReferencesPerType { get; private set; } = [];

        public ValueEdit() { InitializeComponent(); }

        private void OnAvailableReferencesChange()
        {
            if (AvailableReferences == null)
                return;

            Dictionary<EnumDataType, List<GenericReference>> references = [];
            foreach (var reference in AvailableReferences)
            {
                if (references.TryGetValue(reference.DataType, out List<GenericReference>? value))
                    value.Add(reference);
                else
                    references[reference.DataType] = [reference];
            }

            AvailableReferencesPerType = references;
        }
    }
}