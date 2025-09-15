using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Inputs
{
    public class Dropdown : ContentControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header),
            typeof(object),
            typeof(Dropdown),
            new FrameworkPropertyMetadata(null));
        #endregion

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public Dropdown()
        {
        }
    }
}
