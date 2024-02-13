using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfComponents.Lib.Components.FileExplorer.Controls;
using WpfComponents.Lib.Components.FileExplorer.Data;
using System.Linq;

namespace WpfComponents.Lib.Components.FileExplorer
{
    /// <summary>
    /// Interaction logic for FileExplorerList.xaml
    /// </summary>
    public partial class FileExplorerList : FileExplorerBase, IMediatorSortDirection
    {
        public FileExplorerList()
        {
            InitializeComponent();
        }

        public event EventHandler<IEnumerable<ExplorerNode>> SelectionChanged;

        public override IEnumerable<ExplorerNode> SelectedNodes
        {
            get { return ListView.SelectedItems.Cast<ExplorerNode>(); }
        }

        public override PopupActionDnD PopupDnD => PopupTooltipDrag;

        public FileExplorerList() { InitializeComponent(); }

        public static readonly DependencyProperty ScrollViewerParentProperty = DependencyProperty.Register(
    "ScrollViewerParent",
    typeof(ScrollViewer),
    typeof(FileExplorerList),
    new UIPropertyMetadata(null));

        public ScrollViewer ScrollViewerParent
        {
            get { return (ScrollViewer)GetValue(ScrollViewerParentProperty); }
            set { SetValue(ScrollViewerParentProperty, value); }
        }

        #region Methods
        public override void ClearSelection() { ListView.UnselectAll(); }

        protected override void NavigateFolder(ExplorerNodeFolder folderNode)
        {
            base.NavigateFolder(folderNode);
            RootNode = folderNode;
        }

        protected override TextBox GetEditTextBox(ExplorerNode node)
        {
            int index = -1;
            int nodeIndex = -1;
            foreach (var n in RootNode.SortedChildNodes)
            {
                index++;
                if ((ExplorerNode)n == node)
                {
                    nodeIndex = index;
                    break;
                }
            }

            if (nodeIndex < 0)
                return null;

            ListView.UpdateLayout();
            ListViewItem listViewItem = (ListViewItem)ListView.ItemContainerGenerator.ContainerFromIndex(nodeIndex);
            TextBox textBox = VisualTreeHelperExt.GetChildren<TextBox>(listViewItem, true).FirstOrDefault();

            // Set focus on the TextBox and select all text
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
            return textBox;
        }
        #endregion

        #region Events
        protected override void OnRootNodeChange(DependencyPropertyChangedEventArgs eventArgs)
        {
            base.OnRootNodeChange(eventArgs);
            RootNode.Children.CollectionChanged += Children_CollectionChanged;

            // Clean up old events if any
            if (eventArgs.OldValue is ExplorerNodeFolder oldFolderNode)
            {
                // Avoid keeping selected nodes
                foreach (var node in oldFolderNode.Children)
                    node.IsSelected = false;
                oldFolderNode.Children.CollectionChanged -= Children_CollectionChanged;
            }

            ListView.AutoSizeGridViewColumns();
        }

        private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Clear renaming + selection when a file is added/removed
            // This handles file modification with Excel (Excel deletes the file, creates a temp file, recreates the file)
            ClearRenaming();
        }

        protected override void HandlePreviewKeyDown(object sender, KeyEventArgs e)
        {
            base.HandlePreviewKeyDown(sender, e);
            if (e.Handled)
                return;

            // Select all
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.A)
            {
                ListView.SelectAll();
                e.Handled = true;
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _PreventRenaming = true;
            SelectionChanged?.Invoke(this, SelectedNodes);
        }

        // Get keyboard shortcuts
        private void BaseExplorerFile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
            e.Handled = true;
        }

        private void HandleMouseButtons(object sender, MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.XButton1: // Back button
                    NavigateBack();
                    e.Handled = true;
                    break;
                case MouseButton.XButton2: // Forward button
                    NavigateForward();
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Drag mouse selection

        // Based on: https://stackoverflow.com/a/2019638/10404482

        private bool mouseDown = false; // Set to 'true' when mouse is held down.
        private Point? mouseDownPos; // The point where the mouse button was clicked down.

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HandleMouseButtons(sender, e);

            if (e.Handled)
                return;

            FrameworkElement element = e.OriginalSource as FrameworkElement;
            if (element.DataContext is ExplorerNode)
                return;

            // Capture and track the mouse.
            mouseDown = true;
            mouseDownPos = e.GetPosition(GridContainer);
            GridContainer.CaptureMouse();

            // Initial placement of the drag selection box.
            Canvas.SetLeft(selectionBox, mouseDownPos.Value.X);
            Canvas.SetTop(selectionBox, mouseDownPos.Value.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Mouse down can be captured by another control
            if (mouseDownPos == null || !mouseDown)
                return;

            // Release the mouse capture and stop tracking it.
            mouseDown = false;
            GridContainer.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            Point mouseUpPos = e.GetPosition(GridContainer);

            SelectNodesInRectangle(
                new Rect(
                    Math.Min(mouseDownPos.Value.X, mouseUpPos.X),
                    Math.Min(mouseDownPos.Value.Y, mouseUpPos.Y),
                    Math.Max(mouseDownPos.Value.X, mouseUpPos.X) - Math.Min(mouseDownPos.Value.X, mouseUpPos.X),
                    Math.Max(mouseDownPos.Value.Y, mouseUpPos.Y) - Math.Min(mouseDownPos.Value.Y, mouseUpPos.Y)));
            mouseDownPos = null;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown == false || mouseDownPos == null)
                return;

            // When the mouse is held down, reposition the drag selection box.

            Point mousePos = e.GetPosition(GridContainer);

            if (mousePos.X < 0)
                mousePos.X = 0;
            if (mousePos.X > GridContainer.ActualWidth)
                mousePos.X = GridContainer.ActualWidth;
            if (mousePos.Y < 0)
                mousePos.Y = 0;
            if (mousePos.Y > GridContainer.ActualHeight)
                mousePos.Y = GridContainer.ActualHeight;

            if (mouseDownPos.Value.X < mousePos.X)
            {
                Canvas.SetLeft(selectionBox, mouseDownPos.Value.X);
                selectionBox.Width = mousePos.X - mouseDownPos.Value.X;
            }
            else
            {
                Canvas.SetLeft(selectionBox, mousePos.X);
                selectionBox.Width = mouseDownPos.Value.X - mousePos.X;
            }

            if (mouseDownPos.Value.Y < mousePos.Y)
            {
                Canvas.SetTop(selectionBox, mouseDownPos.Value.Y);
                selectionBox.Height = mousePos.Y - mouseDownPos.Value.Y;
            }
            else
            {
                Canvas.SetTop(selectionBox, mousePos.Y);
                selectionBox.Height = mouseDownPos.Value.Y - mousePos.Y;
            }
        }

        private void SelectNodesInRectangle(Rect rectangle)
        {
            ListView.UnselectAll();

            if (rectangle.Width < 10 || rectangle.Height < 10)
                return;

            foreach (var item in ListView.Items)
            {
                var listViewItem = ListView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (listViewItem != null)
                {
                    GeneralTransform gt = listViewItem.TransformToAncestor(GridContainer);
                    var itemBounds = gt.TransformBounds(
                        new Rect(0, 0, listViewItem.ActualWidth, listViewItem.ActualHeight));

                    // Check if item bounds intersect with the selection rectangle
                    if (itemBounds.IntersectsWith(rectangle))
                    {
                        // Select the item
                        listViewItem.IsSelected = true;
                    }
                    else
                    {
                        // Deselect the item
                        listViewItem.IsSelected = false;
                    }
                }
            }
        }
        #endregion

        #region IMediatorSortDirection
        public event EventHandler<ListSortDirection?> OnSortDirectionChange;
        public void Notify(object sender, ListSortDirection? direction)
        { OnSortDirectionChange?.Invoke(sender, direction); }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            SortButton button = sender as SortButton;

            switch (button.Target)
            {
                case "Name":
                    RootNode.SortedChildNodes.CustomSort = new NameComparer(button.SortDirection.Value);
                    break;
                case "DateModified":
                    RootNode.SortedChildNodes.CustomSort = new DateModifiedComparer(button.SortDirection.Value);
                    break;
                case "Size":
                    RootNode.SortedChildNodes.CustomSort = new SizeComparer(button.SortDirection.Value);
                    break;
            }

            RootNode.SortedChildNodes.Refresh();
        }
        #endregion

        #region File explorer bar events
        private void BackExplorer_Click(object sender, RoutedEventArgs e) { NavigateBack(); }

        private void ForwardExplorer_Click(object sender, RoutedEventArgs e) { NavigateForward(); }

        private void ParentExplorer_Click(object sender, RoutedEventArgs e) { NavigateParent(); }

        private void OpenPathExplorer_Click(object sender, RoutedEventArgs e)
        { ExplorerFileCmds.Open.Execute(new List<string>() { RootNode.Path }); }

        private void TextBox_SelectAll(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (!textBox.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    textBox.Focus();
                }
            }
        }
        #endregion
    }
}