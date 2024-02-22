using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers.DnD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Converters;
using System.Windows.Threading;
using WpfComponents.Lib.Components.FileExplorer.Data;
using WpfComponents.Lib.Components.FileExplorer.DnD;
using WpfComponents.Lib.Logic.Helpers;

namespace WpfComponents.Lib.Components.FileExplorer.Controls
{
    /// <summary>
    /// Logique partagé entre ExplorateurFichiersListe et ExplorateurFichiersTree
    /// </summary>
    public abstract class FileExplorerBase : UserControl, INotifyPropertyChanged
    {
        [Flags]
        public enum EnumPermission
        {
            None = 0,
            AllowDrop = 1,
            AllowDrag = 2,
            AllowDragDrop = AllowDrag | AllowDrop,
            AllowShortcuts = 4,
            AllowContextMenuOpen = 8,
            AllowContextMenuActions = 16,
            AllowContextMenu = AllowContextMenuOpen | AllowContextMenuActions,
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        private string _StatusText = "";

        public string StatusText
        {
            get { return _StatusText; }
            set
            {
                _StatusText = value;
                OnPropertyChanged();
            }
        }

        public EnumPermission Permissions
        {
            get;
            set;
        } =
                EnumPermission.AllowDragDrop | EnumPermission.AllowShortcuts | EnumPermission.AllowContextMenu;

        public abstract PopupActionDnD PopupDnD { get; }

        public FileExplorerDnD DnDHandler { get; private set; }

        public TextBox? CurrentRenamingTextBox { get; set; } = null;

        private bool _isNavigating = false;
        private int _currentHistoryIndex = -1;
        private List<ExplorerNodeFolder> _navigationHistory = new List<ExplorerNodeFolder>();
        private readonly DispatcherTimer _renamingTimer;

        // Prevent a click on a node to trigger renaming immediately
        protected bool _preventRenaming { get; set; } = false;

        private ScrollViewer _ScrollParent;

        public abstract IEnumerable<ExplorerNode> SelectedNodes { get; }

        public IEnumerable<string> SelectedPaths { get { return SelectedNodes.Select(x => x.FullPath); } }

        public FileExplorerBase()
        {
            DnDHandler = new FileExplorerDnD(this);
            _renamingTimer = new DispatcherTimer(
                new TimeSpan(0, 0, 0, 0, GetDoubleClickTime()),
                DispatcherPriority.Normal,
                HandleRenamingClick,
                Dispatcher.CurrentDispatcher)
            {
                IsEnabled = false
            };
        }

        #region Dependency Properties
        public static readonly DependencyProperty RootNodeProperty = DependencyProperty.Register(
            "RootNode",
            typeof(ExplorerNodeFolder),
            typeof(FileExplorerBase),
            new UIPropertyMetadata(null, (o, value) => ((FileExplorerBase)o).OnRootNodeChange(value)));

        public ExplorerNodeFolder RootNode
        {
            get { return (ExplorerNodeFolder)GetValue(RootNodeProperty); }
            set { SetValue(RootNodeProperty, value); }
        }

        public static readonly DependencyProperty MediatorProperty = DependencyProperty.Register(
            "Mediator",
            typeof(IMediatorNodeSelected),
            typeof(FileExplorerBase),
            new UIPropertyMetadata(null, (o, value) => ((FileExplorerBase)o).OnMediatorChange()));

        private void OnMediatorChange()
        {
            if (Mediator == null)
                return;
            // If another controller selects an item, we clear our selection
            Mediator.OnNodeSelected += (sender, value) =>
            {
                if (sender != this)
                {
                    ClearSelection();
                }
            };
        }

        public IMediatorNodeSelected Mediator
        {
            get { return (IMediatorNodeSelected)GetValue(MediatorProperty); }
            set { SetValue(MediatorProperty, value); }
        }
        #endregion

        #region Methodes
        public abstract void ClearSelection();

        public void CreateNewFolder(ExplorerNodeFolder targetNode)
        {
            string lNom = FileSystemHelper.GetValidNewFolderName(targetNode.FullPath);

            Directory.CreateDirectory(Path.Combine(targetNode.FullPath, lNom));
            var newFolder = new ExplorerNodeFolder(Path.Combine(targetNode.FullPath, lNom));
            targetNode.Add(newFolder);
            newFolder.IsSelected = true;
            RenameNode(newFolder);
        }
        #endregion

        #region Events
        protected virtual void OnRootNodeChange(DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!_isNavigating)
                AddToHistory(RootNode);
        }

        protected void HandleNodeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (CurrentRenamingTextBox != null)
                return;

            ExplorerNode? targetNode = ((FrameworkElement)e.OriginalSource).DataContext as ExplorerNode;
            if (targetNode == null)
                return;

            _preventRenaming = true;
            NavigateToNodes(e, new List<ExplorerNode>() { targetNode });
            _renamingTimer.Stop();
            e.Handled = true;
        }

        // Si le click est effectuer sur un élément déjà sélectionné et qu'on a pas fait un double click on renomme
        protected void HandleNodeMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_preventRenaming)
            {
                _preventRenaming = false;
                return;
            }
            if (e.ChangedButton != MouseButton.Left)
                return;
            _renamingTimer.Start();
        }

        protected virtual void HandlePreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Not considered as a shortcut you can disallow
            HandleRenamingShortcuts(sender, e);

            if (Permissions.HasFlag(EnumPermission.AllowShortcuts) == false ||
                CurrentRenamingTextBox != null ||
                e.Handled)
                return;

            // Go back 
            if (e.Key == Key.Back)
            {
                NavigateToParent();
                e.Handled = true;
            }
            // Past
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                ExplorerNodeFolder? target = SelectedNodes.FirstOrDefault() as ExplorerNodeFolder;
                if (target == null)
                    target = RootNode;

                if (FileExplorerCmds.PasteFromClipboard.CanExecute(target.FullPath))
                {
                    FileExplorerCmds.PasteFromClipboard.Execute(target.FullPath);
                    e.Handled = true;
                }
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                e.Key == Key.N)
            {
                CreateNewFolder(RootNode);
                e.Handled = true;
            }

            if (!SelectedNodes.Any())
                return;

            // Shortcuts that require a selected node

            // Cut
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                FileExplorerCmds.CutToClipboard.Execute(SelectedPaths);
                e.Handled = true;
            }
            // Copy
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                FileExplorerCmds.CopyToClipboard.Execute(SelectedPaths);
                e.Handled = true;
            }
            // Open
            else if (e.Key == Key.Enter)
            {
                NavigateToNodes(e, SelectedNodes);
            }
            // Remove
            else if (e.Key == Key.Delete)
            {
                if (FileExplorerCmds.Delete.CanExecute(SelectedPaths))
                {
                    FileExplorerCmds.Delete.Execute(SelectedPaths);
                    e.Handled = true;
                }
            }
            // Rename
            else if (e.Key == Key.F2)
            {
                RenameNode(SelectedNodes.First());
                e.Handled = true;
            }
        }

        // Either :
        // - We have multiple TreeViews one after the other, we want a parent scrollviewer to handle them all
        // - We have a single TreeView and we want to let it handle its own scroll without having to add a scrollviewer
        protected void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_ScrollParent == null)
                _ScrollParent = MoreVisualTreeHelper.FindParent<ScrollViewer>(this);

            if (e.Handled == true || _ScrollParent == null)
                return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            _ScrollParent.RaiseEvent(eventArg);
        }
        #endregion

        #region Renommage
        public void RenameNode(ExplorerNode target)
        {
            CurrentRenamingTextBox = GetEditTextBox(target);
            if (CurrentRenamingTextBox == null)
                return;

            if (target is ExplorerNodeFolder)
            {
                CurrentRenamingTextBox.SelectAll();
            }
            else
            {
                // Set selection on the name without the extension
                int lTailleNomFichier = target.Name.LastIndexOf(".");
                if (lTailleNomFichier > 0)
                    CurrentRenamingTextBox.Select(0, lTailleNomFichier);
                else
                    CurrentRenamingTextBox.SelectAll();
            }

            target.IsEditing = true;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        protected static extern int GetDoubleClickTime();

        protected void ClearRenaming()
        {
            if (CurrentRenamingTextBox == null)
                return;
            var currentNode = (ExplorerNode)CurrentRenamingTextBox.DataContext;
            currentNode.IsEditing = false;
            CurrentRenamingTextBox.Select(0, 0);
            CurrentRenamingTextBox = null;
            // Fix weird selection behavior after renaming (element selected forever ?)
            ClearSelection();
        }

        protected void CancelRenaming()
        {
            if (CurrentRenamingTextBox == null)
                return;
            var target = (ExplorerNode)CurrentRenamingTextBox.DataContext;
            CurrentRenamingTextBox.Text = target.Name;
            ClearRenaming();
        }

        protected abstract TextBox GetEditTextBox(ExplorerNode target);

        protected void TextBoxNode_LostFocus(object sender, RoutedEventArgs e)
        {
            var lTextBox = (TextBox)sender;
            var lNode = (ExplorerNode) lTextBox.DataContext;

            if (lNode.IsEditing == false)
                return;

            lNode.IsEditing = false;

            // If the name is the same, we don't do anything
            if (lTextBox.Text != lNode.Name)
            {
                var lParams = new FileParams(
                    lNode.FullPath,
                    Path.Combine(Path.GetDirectoryName(lNode.FullPath), lTextBox.Text));
                if (FileExplorerCmds.Rename.CanExecute(lParams) == false)
                {
                    CancelRenaming();
                    MessageBox.Show(
                        "A file with the same name already exist at this location.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    FileExplorerCmds.Rename.Execute(lParams);
                } catch (OperationCanceledException)
                {
                    CancelRenaming();
                    return;
                }
            }

            ClearRenaming();
            e.Handled = true;
        }

        protected void NameNode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Only allow valid characters
            Regex regex = new Regex(@"^[\w\-. ]+$");
            if (!regex.IsMatch(e.Text))
                e.Handled = true;
            base.OnPreviewTextInput(e);
        }


        private void HandleRenamingShortcuts(object sender, KeyEventArgs e)
        {
            if (CurrentRenamingTextBox == null)
                return;

            if (e.Key == Key.Escape)
            {
                CancelRenaming();
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                // Force focus on the control so that the LostFocus event is called on the TextBox
                this.Focus();
                e.Handled = true;
            }
        }

        private void HandleRenamingClick(object sender, EventArgs e)
        {
            // Safeguard
            if (SelectedNodes.Any() == false)
                return;

            _renamingTimer.Stop();
            RenameNode(SelectedNodes.First());
        }
        #endregion

        #region Navigation
        protected void NavigateToNodes(RoutedEventArgs e, IEnumerable<ExplorerNode> nodes)
        {
            if (nodes.Count() == 0)
                return;

            var firstNode = nodes.First();

            if (firstNode is ExplorerNodeFolder folder)
            {
                NavigateToFolder(folder);
                e.Handled = true;
            }
            // If file, open all files
            else if (firstNode is ExplorerNodeFile)
            {
                FileExplorerCmds.Open
                    .Execute(nodes.Where(x => x is ExplorerNodeFile).Select(x => x.FullPath));
                e.Handled = true;
            }
        }

        protected virtual void NavigateToFolder(ExplorerNodeFolder folder)
        {
            folder.IsOpen = true;
        }

        public void NavigateToParent()
        {
            ExplorerNode parentNode = SelectedNodes.FirstOrDefault()?.Parent ?? RootNode;

            // On remonte le parent du parent
            if (parentNode.Parent == null)
                return;

            NavigateToFolder(parentNode.Parent);
        }
        #endregion

        #region Navigation history
        protected void AddToHistory(ExplorerNodeFolder target)
        {
            if (_currentHistoryIndex < _navigationHistory.Count - 1)
            {
                _navigationHistory.RemoveRange(
                    _currentHistoryIndex + 1,
                    _navigationHistory.Count - _currentHistoryIndex - 1);
            }
            _navigationHistory.Add(target);
            _currentHistoryIndex = _navigationHistory.Count - 1;
        }

        public void GoBack()
        {
            if (_currentHistoryIndex - 1 < 0)
                return;

            _currentHistoryIndex -= 1;
            _isNavigating = true;
            RootNode = _navigationHistory[_currentHistoryIndex];
            _isNavigating = false;
        }

        public void GoForward()
        {
            // If nothing in the history or we are already at the end
            if (_currentHistoryIndex + 1 >= _navigationHistory.Count)
                return;

            _currentHistoryIndex += 1;

            _isNavigating = true;
            RootNode = _navigationHistory[_currentHistoryIndex];
            _isNavigating = false;
        }
        #endregion

        #region DragDrop events

        // XXX : Peut être il y a un moyen d'appeler un event Handler dans une prop (GestionDnD.HandleDragMouseDown directement dans le XAML) mais je crois pas

        protected void HandleMouseDown(object sender, MouseButtonEventArgs e)
        { DnDHandler.HandleDragMouseDown(sender, e); }

        protected void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        { DnDHandler.HandleDragMouseMove(sender, e); }

        protected void HandleDragOver(object sender, DragEventArgs e) { DnDHandler.HandleDragOver(sender, e); }

        protected void HandleDragLeave(object sender, DragEventArgs e) { DnDHandler.HandleDragLeave(sender, e); }

        protected void HandleDrop(object sender, DragEventArgs e) { DnDHandler.HandleDrop(sender, e); }
        #endregion

        #region Contexte menu
        protected virtual void ContextMenu_OnOpening(object sender, ContextMenuEventArgs e)
        {
            var source = e.Source as FrameworkElement;
            var originalSource = e.OriginalSource as FrameworkElement;
            ExplorerNode targetNode = SelectedNodes.FirstOrDefault() ?? RootNode;

            if ((Permissions & EnumPermission.AllowContextMenu) == 0)
            {
                source.ContextMenu.IsOpen = false;
                e.Handled = true;
                return;
            }

            // Permet de gérer un clique droit hors de la TreeView (étant donnés que les éléments restent sélectionnés)
            if (originalSource?.DataContext == null)
                targetNode = RootNode;

            bool foreOpenning = source.ContextMenu == null;
            source.ContextMenu = new ContextMenuExplorer(this, targetNode);

            // During the first opening we force the opening (otherwise the ContextMenu is not ready at the time of display)
            if (foreOpenning)
            {
                source.ContextMenu.IsOpen = true;
                e.Handled = true;
            }
        }
        #endregion
    }
}
