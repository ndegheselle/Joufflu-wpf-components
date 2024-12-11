using Joufflu.Inputs.Format;
using System.ComponentModel;
using System.Windows;

namespace Joufflu.Inputs
{
    public partial class NumericUpDown : SingleValueFormatTextBox<int>, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(default(int), (o, e) => ((NumericUpDown)o).OnValueChanged(e)
        ));

        public override int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
    }
}
