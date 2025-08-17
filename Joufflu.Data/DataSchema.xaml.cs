using Joufflu.Shared.Resources.Fonts;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Usuel.Shared;
using Usuel.Shared.Data;

namespace Joufflu.Data
{
    public class DepthToMarginConverter : IValueConverter
    {
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">bool, indicate wheter the result should be true or false</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is not uint depth)
                return null;
            return new Thickness((depth - 1) * 16, 0, 0, 0);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class ValueTypeIcon : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
            nameof(Type),
            typeof(EnumValueType),
            typeof(ValueTypeIcon),
            new PropertyMetadata(EnumValueType.String, (d, e) => ((ValueTypeIcon)d).OnTypeChanged()));

        public EnumValueType Type
        {
            get { return (EnumValueType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string Icon { get; set; } = IconFont.QuoteLeft;

        private void OnTypeChanged()
        {
            switch(Type)
            {
                case EnumValueType.Object:
                    Icon = IconFont.List;
                    break;
                case EnumValueType.String:
                    Icon = IconFont.QuoteLeft;
                    break;
                case EnumValueType.Decimal:
                    Icon = IconFont.Hashtag;
                    break;
                case EnumValueType.Boolean:
                    Icon = IconFont.Check;
                    break;
                case EnumValueType.DateTime:
                    Icon = IconFont.CalendarDay;
                    break;
                case EnumValueType.TimeSpan:
                    Icon = IconFont.Clock;
                    break;
            }
        }
    }

    /// <summary>
    /// TODO : Use command and a control template
    /// </summary>
    public partial class DataSchema : Control
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(
            nameof(Root),
            typeof(SchemaObject),
            typeof(DataSchema),
            new PropertyMetadata(null));

        public SchemaObject Root
        {
            get { return (SchemaObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }
    }
}