using Joufflu.Inputs.Format;
using System.ComponentModel;
using System.Windows;

namespace Joufflu.Inputs
{
    public partial class DecimalUpDown : SingleValueFormatTextBox<decimal>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(decimal), typeof(DecimalUpDown), new PropertyMetadata(default(decimal), (o, e) => ((DecimalUpDown)o).OnValueChanged(e)
        ));

        public override decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public DecimalUpDown()
        {
            Format = "{decimal|noGlobalSelection}";
        }
    }
}
