using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Schema;

namespace Joufflu.Data.Schema
{
    public class Separators : ContentControl
    {
        public static readonly DependencyProperty ElementProperty =
            DependencyProperty.Register(nameof(Element), typeof(ISchemaElement), typeof(Separators), new PropertyMetadata(null));

        public ISchemaElement Element
        {
            get { return (ISchemaElement)GetValue(ElementProperty); }
            set { SetValue(ElementProperty, value); }
        }
    }
}
