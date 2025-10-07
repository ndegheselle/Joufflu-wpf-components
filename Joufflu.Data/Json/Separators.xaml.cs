using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Json
{
    public class Separators : ContentControl
    {
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(nameof(Element), typeof(object), typeof(Json.Separators), new PropertyMetadata(null));

        public object Element
        {
            get { return (object)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }
    }
}
