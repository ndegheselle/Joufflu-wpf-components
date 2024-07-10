using Joufflu.Inputs.Components.Format;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Joufflu.Inputs.Components
{
    public partial class TimeSpanPicker : SingleValueFormatTextBox<TimeSpan?>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(TimeSpan?), typeof(TimeSpanPicker), new PropertyMetadata(default(TimeSpan?), (o, e) => ((TimeSpanPicker)o).OnValueChanged(e)
        ));

        public override TimeSpan? Value
        {
            get { return (TimeSpan?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public override TimeSpan? ConvertFrom()
        {
            if (Values.Count() < 4)
                return null;

            int? hour = Values[0] as int?;
            int? minute = Values[1] as int?;
            int? second = Values[2] as int?;
            int? millisecond = Values[3] as int?;

            if (!hour.HasValue || !minute.HasValue || !second.HasValue || !millisecond.HasValue)
                return null;

            return new TimeSpan(hour.Value, minute.Value, second.Value, millisecond.Value);
        }

        public override List<object?> ConvertTo()
        {
            if (Value is TimeSpan date)
                return new List<object?>() { date.Days, date.Hours, date.Minutes, date.Seconds };
            else
                return new List<object?>() { null, null, null, null };
        }
    }
}
