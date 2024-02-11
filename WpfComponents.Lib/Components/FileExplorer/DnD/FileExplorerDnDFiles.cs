using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WpfComponents.Lib.Components.FileExplorer;
using WpfComponents.Lib.Components.FileExplorer.Controls;

namespace GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers.DnD
{
    // XXX : could have a common interface with FileExplorerDnDBase
    internal class FileExplorerDnDFiles
    {
        private readonly FileExplorerBase _explorer;
        private readonly IDataObject _dataObject;

        public FileExplorerDnDFiles(FileExplorerBase explorer, IDataObject dataObject)
        {
            _explorer = explorer;
            _dataObject = dataObject;
        }

        public bool IsValid(string destinationFolder)
        {
            string[]? sourceFilePaths = _dataObject.GetData(DataFormats.FileDrop) as string[];
            // Nothing to drag
            if (sourceFilePaths == null)
            {
                return false;
            }

            // Check if the copy is allowed
            foreach (string sourceFilePath in sourceFilePaths)
            {
                if (!FileExplorerCmds.Copy.CanExecute(new FileParams(sourceFilePath, Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath)))))
                {
                    return false;
                }
            }

            return true;
        }

        public void GetFiles(string destinationFolder)
        {
            string[]? sourceFilePaths = _dataObject.GetData(DataFormats.FileDrop) as string[];

            if (sourceFilePaths == null) return;

            // Copy if SHIFT else Cut
            var isCopy = IsCopy();
            foreach (string sourceFilePath in sourceFilePaths)
            {
                try
                {
                    if (isCopy)
                        FileExplorerCmds.Copy.Execute(new FileParams(sourceFilePath, Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath))));
                    else
                        FileExplorerCmds.Cut.Execute(new FileParams(sourceFilePath, Path.Combine(destinationFolder, Path.GetFileName(sourceFilePath))));
                }
                catch (OperationCanceledException)
                { }
            }
        }

        public bool IsCopy()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }
    }
}
