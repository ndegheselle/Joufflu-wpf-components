using AdonisUI.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;

namespace Joufflu.Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow, INotifyPropertyChanged
    {
        public MainWindow()
        {
            string tata = "afaf".ToString();

            InitializeComponent();
            // Foreach tab in MainContainer create a ListBoxItem and add it to the ListBox
            foreach (TabItem tab in MainContainer.Items)
            {
                var item = new ListBoxItem { Content = tab.Header, Tag = tab };
                item.Selected += (s, e) =>
                {
                    MainContainer.SelectedItem = (TabItem)((ListBoxItem)s).Tag;
                };
                SideMenu.Items.Add(item);
            }
        }
    }
}