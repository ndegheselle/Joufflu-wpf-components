using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers.DnD;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfComponents.Lib.Components.FileExplorer.Controls;
using WpfComponents.Lib.Components.FileExplorer.Data;
using WpfComponents.Lib.Logic;
using static WpfComponents.Lib.Components.FileExplorer.Controls.FileExplorerBase;

namespace WpfComponents.Lib.Components.FileExplorer.DnD
{
    /// <summary>
    /// Allows handling Drag and Drop in the file explorer
    /// Handles files, zip files, and Outlook attachments
    /// </summary>
    public class FileExplorerDnD : BaseDnDHandler
    {
        private readonly FileExplorerBase _explorer;

        public FileExplorerDnD(FileExplorerBase explorer) : base(
            explorer,
            explorer.PopupDnD)
        { _explorer = explorer; }

        #region Event handling

        protected override object GetSourceData(FrameworkElement source)
        {
            return new DataObject(DataFormats.FileDrop, _explorer.SelectedPaths.ToArray());
        }

        // XXX : could have a common interface with FileExplorerDnDBase and loop on all handlers
        protected override void ApplyDrop(object sender, DragEventArgs e)
        {
            ExplorerNodeFolder? dataContext = ((FrameworkElement)e.OriginalSource).DataContext as ExplorerNodeFolder;
            if (dataContext == null)
                dataContext = _explorer.RootNode;

            FileExplorerDnDOutlook lOutlook = new FileExplorerDnDOutlook(_explorer, e.Data);
            if (lOutlook.IsValid())
            {
                lOutlook.GetFiles(dataContext.FullPath);
                return;
            }

            FileExplorerDnDZip lZip = new FileExplorerDnDZip(_explorer, e.Data);
            if (lZip.IsValid())
            {
                lZip.GetFiles(dataContext.FullPath);
                return;
            }

            FileExplorerDnDFiles lFichier = new FileExplorerDnDFiles(_explorer, e.Data);
            lFichier.GetFiles(dataContext.FullPath);
        }

        protected override bool CanDrop(object sender, DragEventArgs args)
        {
            if (_explorer.Permissions.HasFlag(EnumPermission.AllowDrop) == false)
                return false;

            FrameworkElement? target = args.OriginalSource as FrameworkElement;
            ExplorerNode? dataContext = target?.DataContext as ExplorerNode;

            if (dataContext == null)
                dataContext = _explorer.RootNode;

            if (dataContext is ExplorerNodeFolder == false)
            {
                return false;
            }

            FileExplorerDnDOutlook lOutlook = new FileExplorerDnDOutlook(_explorer, args.Data);
            FileExplorerDnDZip lZip = new FileExplorerDnDZip(_explorer, args.Data);
            FileExplorerDnDFiles lFichier = new FileExplorerDnDFiles(_explorer, args.Data);
            if (!lOutlook.IsValid() && !lZip.IsValid() && !lFichier.IsValid(dataContext.FullPath))
            {
                return false;
            }

            // Update popup content
            if (lFichier.IsCopy())
                _explorer.PopupDnD.ChangeEffect(DragDropEffects.Copy);
            else
                _explorer.PopupDnD.ChangeEffect(DragDropEffects.Move);

            return true;
        }

        protected override bool CanDrag(object sender, MouseEventArgs args)
        {
            if (_explorer.Permissions.HasFlag(EnumPermission.AllowDrag) == false ||
                _explorer.EditingTextBox != null)
                return false;

            // Handle continuation after cancellation (prevent starting another drag directly)
            FrameworkElement? lElement = args.OriginalSource as FrameworkElement;
            if (lElement?.DataContext is ExplorerNode == false ||
                !_explorer.SelectedNodes.Contains(lElement.DataContext))
                return false;

            return true;
        }

        #endregion
    }
}
