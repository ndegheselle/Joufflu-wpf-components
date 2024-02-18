using System.ComponentModel;
using System.Windows;
using WpfComponents.Lib.Components.Inputs.Format;

namespace WpfComponents.Lib.Components.Inputs
{
    public partial class NumericUpDown : SingleValueFormatTextBox<long>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(long), typeof(NumericUpDown), new PropertyMetadata(default(long), (o, e) => ((NumericUpDown)o).OnValueChanged(e)
        ));

        public override long Value
        {
            get { return (long)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
