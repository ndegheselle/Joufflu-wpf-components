using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Data.Json
{
    /// <summary>
    /// Logique d'interaction pour JsonEditor.xaml
    /// </summary>
    public partial class JsonEditor : UserControl
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(nameof(Root), typeof(JObject), typeof(JsonEditor), new PropertyMetadata(null));

        public JObject Root { get { return (JObject)GetValue(RootProperty); } set { SetValue(RootProperty, value); } }

        public JsonEditor() { InitializeComponent(); }
    }
}
