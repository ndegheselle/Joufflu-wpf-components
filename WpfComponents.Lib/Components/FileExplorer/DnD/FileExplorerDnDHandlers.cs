using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using WpfComponents.Lib.Components.FileExplorer.Controls;
using WpfComponents.Lib.Components.FileExplorer.DnD;

namespace GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers.DnD
{
    internal class FileExplorerDnDBase
    {
        private readonly FileExplorerBase _explorer;
        private readonly IDataObject _dataObject;
        private readonly string _formatFileNames;
        private readonly string _formatFileContents;

        public FileExplorerDnDBase(FileExplorerBase explorer, IDataObject dataObject, string formatFileNames, string formatFileContents)
        {
            _explorer = explorer;
            _dataObject = dataObject;
            _formatFileNames = formatFileNames;
            _formatFileContents = formatFileContents;
        }

        public bool IsValid()
        {
            object attachment = _dataObject.GetDataPresent(_formatFileNames);
            if (attachment == null || (attachment as bool?) == false)
                return false;
            return true;
        }

        public void GetFiles(string destinationFolder)
        {
            //wrap standard IDataObject in OutlookDataObject
            DragDropDataObject dataObject = new DragDropDataObject(_dataObject);

            //get the names and data streams of the files dropped
            string[] filenames = (string[])dataObject.GetData(_formatFileNames);
            MemoryStream[] filestreams = (MemoryStream[])dataObject.GetData(_formatFileContents);

            Task.Run(() =>
            {
                try
                {
                    for (int fileIndex = 0; fileIndex < filenames.Length; fileIndex++)
                    {
                        //use the fileindex to get the name and data stream
                        string filename = filenames[fileIndex];
                        MemoryStream filestream = filestreams[fileIndex];

                        _explorer.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _explorer.StatusText = $"Retrieving '{filename}' ...";
                        }));

                        //save the file stream using its name to the application path
                        FileStream outputStream = File.Create(Path.Combine(destinationFolder, filename));
                        filestream.WriteTo(outputStream);
                        outputStream.Close();
                    }
                }
                finally
                {
                    _explorer.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _explorer.StatusText = "";
                    }));
                }
            });
        }
    }

    internal class FileExplorerDnDOutlook : FileExplorerDnDBase
    {
        public FileExplorerDnDOutlook(FileExplorerBase explorer, IDataObject dataObject)
            : base(explorer, dataObject, "FileGroupDescriptor", "FileContents")
        {
        }
    }

    internal class FileExplorerDnDZip : FileExplorerDnDBase
    {
        public FileExplorerDnDZip(FileExplorerBase explorer, IDataObject dataObject)
            : base(explorer, dataObject, "FileGroupDescriptorW", "FileContents")
        {
        }
    }
}
