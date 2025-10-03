using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Schema;

namespace Joufflu.Data.Schema.Components
{
    public class Separators : ContentControl
    {
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(nameof(Element), typeof(GenericElement), typeof(Separators), new PropertyMetadata(null));

        public GenericElement Element
        {
            get { return (GenericElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }
    }
}
