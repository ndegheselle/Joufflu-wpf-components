using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfComponents.Lib.Components.Filters.Data;
using static WpfComponents.Lib.Components.Filters.DataFilters;

namespace WpfComponents.Lib.Components.Filters
{
    /// <summary>
    /// Logique d'interaction pour AffichageFiltres.xaml
    /// </summary>
    public partial class FilterRow : UserControl
    {
        #region DependencyProperty
        // Create a DependencyProperty for GestionDnDFiltre
        public static readonly DependencyProperty GestionDnDFiltreProperty = DependencyProperty.Register(
            "GestionDnD",
            typeof(FilterDnDHandler),
            typeof(FilterRow),
            new PropertyMetadata(null));

        public FilterDnDHandler GestionDnD
        {
            get { return (FilterDnDHandler)GetValue(GestionDnDFiltreProperty); }
            set { SetValue(GestionDnDFiltreProperty, value); }
        }

        public static readonly DependencyProperty RootGroupProperty = DependencyProperty.Register(
            "RootGroup",
            typeof(FilterGroup),
            typeof(FilterRow),
            new PropertyMetadata(null, (o, e) => ((FilterRow)o).OnRootChanged()));

        private void OnRootChanged()
        {
            if (RootGroup == null)
                return;

            if (RootGroup.Childrens.Count == 0)
                RootGroup.Childrens.Add(new Filter() { ParentGroup = RootGroup });
        }

        public FilterGroup RootGroup
        {
            get { return (FilterGroup)GetValue(RootGroupProperty); }
            set { SetValue(RootGroupProperty, value); }
        }

        public static readonly DependencyProperty DisplayProperty = DependencyProperty.Register(
            "Display",
            typeof(EnumDisplay),
            typeof(FilterRow),
            new PropertyMetadata(EnumDisplay.Simple));

        public EnumDisplay Display
        {
            get { return (EnumDisplay)GetValue(DisplayProperty); }
            set { SetValue(DisplayProperty, value); }
        }

        #endregion
        public List<EnumConjunctionFilter> ConjunctionOperators { get; }

        public FilterRow()
        {
            InitializeComponent();
            ConjunctionOperators = Enum.GetValues(typeof(EnumConjunctionFilter)).Cast<EnumConjunctionFilter>().ToList();
        }

        #region UI Events
        private void MenuItem_Add_Click(object sender, RoutedEventArgs e)
        {
            var propFilter = (Filter)((FrameworkElement)sender).DataContext;
            var @new = new Filter();
            @new.AddRelativeTo(propFilter, 1);
        }

        private void MenuItem_Remove_Click(object sender, RoutedEventArgs e)
        {
            var propFilter = (Filter)((FrameworkElement)sender).DataContext;
            propFilter.Delete();
        }

        private void MenuItem_Grouper_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuItem_Degrouper_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuItem_GoUp_Click(object sender, RoutedEventArgs e)
        {
            var propFilter = (Filter)((FrameworkElement)sender).DataContext;
            propFilter.Move(-1);
        }

        private void MenuItem_GoDown_Click(object sender, RoutedEventArgs e)
        {
            var propFilter = (Filter)((FrameworkElement)sender).DataContext;
            propFilter.Move(+1);
        }

        private void DisplayActions(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            var contextMenu = (ContextMenu)element.FindResource("ContextMenuFilter");
            contextMenu.PlacementTarget = element;
            contextMenu.IsOpen = true;
        }

        #endregion

        #region DnD
        protected void HandleMouseDown(object sender, MouseButtonEventArgs e)
        { GestionDnD.HandleDragMouseDown(sender, e); }

        protected void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        { GestionDnD.HandleDragMouseMove(sender, e); }

        protected void HandleDragOver(object sender, DragEventArgs e)
        {
            GestionDnD.HandleDragOver(sender, e);
            e.Handled = true;
        }

        protected void HandleDragLeave(object sender, DragEventArgs e)
        {
            GestionDnD.HandleDragLeave(sender, e);

            Border lRow = sender as Border;
            lRow.Padding = new Thickness(0);
            e.Handled = true;
        }

        protected void HandleDrop(object sender, DragEventArgs e)
        {
            GestionDnD.HandleDrop(sender, e);
            Border lRow = sender as Border;
            lRow.Padding = new Thickness(0);
            e.Handled = true;
        }

        private void DataGridRow_DragEnter(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
            Border lRow = sender as Border;
            lRow.Padding = new Thickness(0, 10, 0, 0);
            e.Handled = true;
        }
        #endregion
    }
}
