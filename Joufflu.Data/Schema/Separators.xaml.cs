using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Schema;

namespace Joufflu.Data.Schema
{
    public class Separators : ContentControl
    {
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(nameof(Element), typeof(IGenericElement), typeof(Separators), new PropertyMetadata(null));

        public IGenericElement Element
        {
            get { return (IGenericElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }
    }
}
