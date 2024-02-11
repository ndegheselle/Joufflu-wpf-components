using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfComponents.Lib.Components.FileExplorer.Data
{
    public interface IMediateurSelectionFichier
    {
        EventHandler<IEnumerable<ExplorerNode>> OnSelectionNode { get; set; }
        void Notifier(object source, IEnumerable<ExplorerNode> nodes);
    }

    public class NodeExplorerObserver : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Properties

        private Dictionary<string, ExplorerContext> _Contexts;
        public Dictionary<string, ExplorerContext> Contexts
        {
            get { return _Contexts; }
            set
            {
                _Contexts = value;
                AddContexts();
            }
        }

        private ExplorerNodeFolder _RootNode;
        public ExplorerNodeFolder RootNode
        {
            get { return _RootNode; }
            set
            {
                _RootNode = value;
                OnPropertyChanged();
            }
        }

        private FileSystemWatcher _Watcher;

        #endregion

        public NodeExplorerObserver(
            ExplorerNodeFolder folderNode,
            Dictionary<string, ExplorerContext> contexts = null
        )
        {
            RootNode = folderNode;
            Contexts = contexts ?? new Dictionary<string, ExplorerContext>();
            InitializeFileSystemWatcher(RootNode.FullPath);
        }

        public void Dispose()
        {
            _Watcher.Dispose();
        }

        private void InitializeFileSystemWatcher(string targetFolder)
        {
            _Watcher = new FileSystemWatcher();

            _Watcher.Path = targetFolder;
            _Watcher.IncludeSubdirectories = true;
            _Watcher.Filter = "*.*";
            _Watcher.NotifyFilter =
                NotifyFilters.LastAccess
                | NotifyFilters.LastWrite
                | NotifyFilters.FileName
                | NotifyFilters.DirectoryName;

            _Watcher.Created += OnFileCreation;
            _Watcher.Deleted += OnFileDeletion;
            _Watcher.Renamed += OnFileRenaming;
            _Watcher.EnableRaisingEvents = true;
        }

        private void AddContexts()
        {
            foreach (var contextEntry in _Contexts)
            {
                var node = RootNode.SearchChildNode(contextEntry.Key);
                if (node == null)
                    continue;
                node.Context = contextEntry.Value;
            }
        }

        private void AddContextIfExists(ExplorerNode node)
        {
            if (!Contexts.ContainsKey(node.FullPath))
                return;
            node.Context = Contexts[node.FullPath];
        }

        #region File system observation

        private void OnFileCreation(object sender, FileSystemEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application
                    .Current
                    .Dispatcher
                    .Invoke(new System.Action(() => OnFileCreation(sender, e)));
                return;
            }

            var watcher = ((FileSystemWatcher)sender);

            var folderNode =
                RootNode.SearchChildNode(Directory.GetParent(e.FullPath).FullName)
                as ExplorerNodeFolder;
            if (folderNode == null)
                return;

            // If the node was manually created, we don't recreate it (e.g. for creating a new folder)
            var targetNode = folderNode.SearchChildNode(e.FullPath);
            if (targetNode != null)
                return;

            ExplorerNode node = targetNode;
            if (Directory.Exists(e.FullPath))
            {
                var childFolderNode = new ExplorerNodeFolder(e.FullPath);
                node = childFolderNode;
                childFolderNode.UpdateChildren(0);
            }
            else
                node = new ExplorerNodeFile(e.FullPath);

            AddContextIfExists(node);
            folderNode.Add(node);
            // Refresh the sorting
            folderNode.Refresh();
        }

        private void OnFileDeletion(object sender, FileSystemEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application
                    .Current
                    .Dispatcher
                    .Invoke(new System.Action(() => OnFileDeletion(sender, e)));
                return;
            }

            var folderNode =
                RootNode.SearchChildNode(Directory.GetParent(e.FullPath).FullName)
                as ExplorerNodeFolder;
            if (folderNode == null)
                return;

            folderNode
                .Children
                .Where(x => x.FullPath == e.FullPath)
                .ToList()
                .ForEach(x => folderNode.Children.Remove(x));
        }

        private void OnFileRenaming(object sender, RenamedEventArgs e)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application
                    .Current
                    .Dispatcher
                    .Invoke(new System.Action(() => OnFileRenaming(sender, e)));
                return;
            }

            var targetNode = RootNode.SearchChildNode(e.OldFullPath);
            if (targetNode == null)
                return;

            targetNode.FullPath = e.FullPath;

            // If it's a folder, refresh the children (since their paths will be messed up)
            if (targetNode is ExplorerNodeFolder folderNode)
                folderNode.UpdateChildren(1, true);

            if (targetNode.Parent == null)
                return;

            // Refresh the sorting of the parent folder
            targetNode.Parent.Refresh();
        }

        #endregion
    }

    public class FileExplorerController
        : IDisposable,
            INotifyPropertyChanged,
            IMediateurSelectionFichier
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #region Properties

        private ExplorerNodeFolder _SelectedFolder;
        public ExplorerNodeFolder SelectedFolder
        {
            get { return _SelectedFolder; }
            set
            {
                _SelectedFolder = value;
                OnPropertyChanged();
            }
        }

        // NOTE: /!\ Managed resource, needs to be cleaned up
        public ObservableCollection<NodeExplorerObserver> Observers { get; set; } =
            new ObservableCollection<NodeExplorerObserver>();

        #endregion

        #region Initialization & clean up

        public ExplorerNodeFolder AddFolder(
            string targetFolder,
            Dictionary<string, ExplorerContext> contexts = null
        )
        {
            var rootNode = new ExplorerNodeFolder(targetFolder);
            rootNode.UpdateChildren(1, true);

            SelectedFolder = rootNode;
            Observers.Add(new NodeExplorerObserver(rootNode, contexts));
            return rootNode;
        }

        public void Dispose()
        {
            // Make sure that instances of the FileWatcher stop running
            foreach (var observer in Observers)
            {
                observer.Dispose();
            }
        }

        #endregion

        #region Navigation

        public void SelectNodes(IEnumerable<string> nodePaths)
        {
            foreach (var observer in Observers)
            {
                foreach (var path in nodePaths)
                {
                    ExplorerNode currentNode = observer
                        .RootNode
                        .SearchChildNode(path);

                    if (currentNode == null)
                        break;

                    if (currentNode is ExplorerNodeFolder currentFolderNode)
                    {
                        currentFolderNode.IsOpen = true;
                    }
                    SelectedFolder = currentNode.Parent;
                    currentNode.IsSelected = true;
                }
            }
        }

        #endregion

        #region IMediateurSelectionFichier

        public EventHandler<IEnumerable<ExplorerNode>> OnSelectionNode { get; set; }

        public void Notifier(object source, IEnumerable<ExplorerNode> nodes)
        {
            OnSelectionNode.Invoke(source, nodes);
        }

        #endregion
    }
}