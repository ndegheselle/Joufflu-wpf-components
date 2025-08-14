using System.Windows;
using System.Windows.Controls;
using Usuel.Shared.Data;

namespace Joufflu.Data
{
    /// <summary>
    /// TODO : Use command and a control template
    /// </summary>
    public partial class DataDefinition : Control
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(nameof(Root), typeof(DataProxyObject), typeof(DataDefinition), new PropertyMetadata(null));

        public DataProxyObject Root
        {
            get { return (DataProxyObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }
    }
}