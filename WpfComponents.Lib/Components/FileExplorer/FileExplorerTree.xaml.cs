using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfComponents.Lib.Components.FileExplorer.Controls;
using WpfComponents.Lib.Components.FileExplorer.Data;
using WpfComponents.Lib.Layout;
using WpfComponents.Lib.Logic.Helpers;

namespace WpfComponents.Lib.Components.FileExplorer
{
    /// <summary>
    /// Interaction logic for FileExplorer.xaml
    /// </summary>
    public partial class FileExplorerTree : FileExplorerBase
    {
        [Flags]
        public enum EnumExplorateurDisplay
        {
            None = 1,
            File = 2,
            Folder = 4,
            NoSorting = 8,
            RootFolder = 16,
            All = File | Folder
        }

        #region Property Changed
        private ExplorerNodeFolder _parentNode = null;

        public ExplorerNodeFolder ParentNode
        {
            get { return _parentNode ?? RootNode; }
            set
            {
                _parentNode = value;
                if (_parentNode != null)
                {
                    _parentNode.Children.Add(RootNode);
                }
                OnPropertyChanged();
            }
        }

        private ExplorerNode _selectedNode = null;

        public ExplorerNode SelectedNode
        {
            get { return _selectedNode; }
            set
            {
                _selectedNode = value;
                // Update the view
                if (_selectedNode != null)
                    _selectedNode.IsSelectedUnique = true;
                if (value is ExplorerNodeFolder && SelectedNode != SelectedFolder)
                    SelectedFolder = value as ExplorerNodeFolder;

                OnPropertyChanged();
            }
        }

        public override IEnumerable<ExplorerNode> SelectedNodes
        {
            get { return new List<ExplorerNode>() { SelectedNode }; }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty SelectedFolderProperty = DependencyProperty.Register(
            "SelectedFolder",
            typeof(ExplorerNodeFolder),
            typeof(FileExplorerTree),
            new UIPropertyMetadata(null, (o, value) => ((FileExplorerTree)o).OnSelectedFolderChange()));

        public ExplorerNodeFolder SelectedFolder
        {
            get { return (ExplorerNodeFolder)GetValue(SelectedFolderProperty); }
            set { SetValue(SelectedFolderProperty, value); }
        }

        private void OnSelectedFolderChange()
        {
            if (SelectedFolder != SelectedNode)
                SelectedNode = SelectedFolder;
        }

        public static readonly DependencyProperty DisplayProperty = DependencyProperty.Register(
            "Display",
            typeof(EnumExplorateurDisplay),
            typeof(FileExplorerTree),
            new UIPropertyMetadata(
                EnumExplorateurDisplay.Folder,
                (o, value) => ((FileExplorerTree)o).OnDisplayChange()));

        public EnumExplorateurDisplay Display
        {
            get { return (EnumExplorateurDisplay)GetValue(DisplayProperty); }
            set { SetValue(DisplayProperty, value); }
        }

        public override PopupActionDnD PopupDnD => PopupTooltipDrag;

        protected override void OnRootNodeChange(DependencyPropertyChangedEventArgs eventArgs)
        {
            base.OnRootNodeChange(eventArgs);
            OnDisplayChange();
        }

        #endregion

        public FileExplorerTree()
        {
            // DragDropFile = new DragDropFile(RootNode);
            InitializeComponent();
        }

        #region Events
        private void OnDisplayChange()
        {
            if (RootNode == null)
                return;

            if (Display.HasFlag(EnumExplorateurDisplay.RootFolder))
                ParentNode = new ExplorerNodeFolder(Path.GetDirectoryName(RootNode.FullPath));
            else
                ParentNode = null;

            RefreshViewNodes(RootNode);
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem? tvi = e.OriginalSource as TreeViewItem;
            ExplorerNodeFolder? folderNode = tvi?.DataContext as ExplorerNodeFolder;

            if (folderNode == null)
                return;
            folderNode.UpdateChildren();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedNode = e.NewValue as ExplorerNode;
            // Notify other treeviews to clear their selections
            if (SelectedNode != null && Mediator != null)
                Mediator.Notify(this, SelectedNodes);

            if (SelectedNode is ExplorerNodeFolder folder && folder.IsOpen == false)
                folder.UpdateChildren();
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
        #endregion

        #region Methods

        // Refresh all nodes to update filters / sorting
        private void RefreshViewNodes(ExplorerNodeFolder folderNode)
        {
            folderNode.Refresh();
            foreach (var child in folderNode.Children)
            {
                var childFolder = child as ExplorerNodeFolder;
                if (childFolder != null)
                    RefreshViewNodes(childFolder);
            }
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        protected override TextBox GetEditTextBox(ExplorerNode node)
        {
            // XXX: There is a good chance that there will be a crash if textBox = null (check how it is handled in the ListView if that's the case)
            StretchingTreeViewItem treeViewItem = (StretchingTreeViewItem)TreeView.ItemContainerGenerator
                .ContainerFromItem(TreeView.SelectedItem);
            TextBox textBox = MoreVisualTreeHelper.GetChildren<TextBox>(treeViewItem, true).FirstOrDefault();

            // Set focus on the TextBox and select all text
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
            return textBox;
        }

        public override void ClearSelection()
        {
            var node = TreeView.SelectedItem as ExplorerNode;
            if (node == null)
                return;

            node.IsSelectedUnique = false;
        }
        #endregion

        #region Drag and drop
        private void Folder_DragOver(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);

            if (e.Handled)
                return;

            // Expand the folder using Binding
            FrameworkElement destination = e.OriginalSource as FrameworkElement;
            ExplorerNodeFolder dataContext = destination.DataContext as ExplorerNodeFolder;

            if (dataContext != null)
                dataContext.IsOpen = true;
        }
        #endregion
    }
}