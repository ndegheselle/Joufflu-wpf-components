using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfComponents.Lib.Components.FileExplorer.Data;

namespace WpfComponents.Lib.Components.FileExplorer
{
    /// <summary>
    /// Logique d'interaction pour FileExplorer.xaml
    /// </summary>
    public partial class FileExplorer : UserControl
    {
        // Dependency property for the root paths of the file explorer
        public static readonly DependencyProperty RootPathsProperty = DependencyProperty.Register(
            "RootPaths",
            typeof(IEnumerable<string>),
            typeof(FileExplorer),
            new PropertyMetadata(null, (o, e) => ((FileExplorer)o).OnRootPathChanged()));

        public IEnumerable<string> RootPaths
        {
            get { return (IEnumerable<string>)GetValue(RootPathsProperty); }
            set { SetValue(RootPathsProperty, value); }
        }

        private void OnRootPathChanged()
        {
            foreach(var observer in Controller.Observers)
            {
                observer.Dispose();
            }
            Controller.Observers.Clear();

            foreach (string path in RootPaths)
            {
                Controller.AddFolder(path);
            }
        }

        public FileExplorerController Controller { get; set; } = new FileExplorerController();

        public FileExplorer() { InitializeComponent(); }
    }
}
