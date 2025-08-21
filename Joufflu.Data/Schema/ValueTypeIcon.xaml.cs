using Joufflu.Shared.Resources.Fonts;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Data;

namespace Joufflu.Data.Schema
{
    public class ValueTypeIcon : Control, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register(
            nameof(Type),
            typeof(EnumValueType),
            typeof(ValueTypeIcon),
            new PropertyMetadata(EnumValueType.String, (d, e) => ((ValueTypeIcon)d).OnTypeChanged()));

        /// <summary>
        /// Type of the value represented by this icon.
        /// </summary>
        public EnumValueType Type
        {
            get { return (EnumValueType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public string Icon { get; set; } = IconFont.QuoteLeft;

        /// <summary>
        /// Change the icon based on the type of value.
        /// </summary>
        private void OnTypeChanged()
        {
            switch (Type)
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
}
