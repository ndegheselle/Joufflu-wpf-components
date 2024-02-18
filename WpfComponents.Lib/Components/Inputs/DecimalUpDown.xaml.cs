using System.ComponentModel;
using System.Windows;
using WpfComponents.Lib.Components.Inputs.Format;

namespace WpfComponents.Lib.Components.Inputs
{
    public partial class DecimalUpDown : SingleValueFormatTextBox<double>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(double), typeof(DecimalUpDown), new PropertyMetadata(default(double), (o, e) => ((DecimalUpDown)o).OnValueChanged(e)
        ));

        public override double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
