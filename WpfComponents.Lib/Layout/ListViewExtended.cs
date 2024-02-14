using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfComponents.Lib.Layout
{
    /// <summary>
    /// https://stackoverflow.com/a/23151248/10404482
    /// Allows drag & drop in a ListView
    /// By default, if we try to DnD an already selected item, the selection is lost
    /// </summary>
    public class ListViewExtended : ListView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListViewItemExtended();
        }

        // Updates the width of columns in a GridView when the ItemSource changes
        public void AutoSizeGridViewColumns()
        {
            GridView gridView = this.View as GridView;
            if (gridView != null)
            {
                foreach (var column in gridView.Columns)
                {
                    if (double.IsNaN(column.Width))
                        column.Width = column.ActualWidth;
                    column.Width = double.NaN;
                }
            }
        }
    }

    public class ListViewItemExtended : ListViewItem
    {
        private bool _deferSelection = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && IsSelected)
            {
                // the user may start a drag by clicking into selected items
                // delay destroying the selection to the Up event
                _deferSelection = true;
            }
            else
            {
                base.OnMouseLeftButtonDown(e);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_deferSelection)
            {
                try
                {
                    base.OnMouseLeftButtonDown(e);
                }
                finally
                {
                    _deferSelection = false;
                }
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            // abort deferred Down
            _deferSelection = false;
            base.OnMouseLeave(e);
        }
    }
}
