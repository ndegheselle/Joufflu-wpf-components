using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using WpfComponents.Lib.Logic;
using WpfComponents.Lib.Logic.Helpers;
using WpfComponents.Lib.Logic.Windows;

namespace WpfComponents.Lib.Components.FileExplorer
{
    // Time to rant :
    // - Impossible de binder une commande directement depuis une classe statique avec x:Static
    // - Binder depuis un context menu depuis un template est un enfer (-> context menu n'est pas dans le même arbre visuel que le reste + le datatemplate aussi)
    // - Quand on bind une commande il FAUT mettre le CommandParameter avant Command dans les attributs XAML
    // - {Binding Chemin} ne marche pas il faut utiliser {Binding DataContext.Chemin, RelativeSource={RelativeSource Self}}
    // what a shitshow ...

    public class FileParams
    {
        public string SourcePath { get; set; }

        public string TargetPath { get; set; }

        public FileParams(string sourcePath, string targetPath)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
        }
    }

    internal class FileExplorerCmds
    {
        private enum EnumDropEffectPressePapier
        {
            Copy,
            Cut,
        }

        public static readonly ICommand Cut = new SimpleCommand<FileParams>(CutInternal, IsSourceTargetValid);
        public static readonly ICommand Copy = new SimpleCommand<FileParams>(CopyInternal, IsSourceTargetValid);
        public static readonly ICommand Rename = new SimpleCommand<FileParams>(CutInternal, IsTargetFree);

        public static readonly ICommand CopyToClipboard = new SimpleCommand<IEnumerable<string>>(
            CopyToClipboardInternal,
            DoesFilesOrFoldersExist);
        public static readonly ICommand CutToClipboard = new SimpleCommand<IEnumerable<string>>(
            CutToClipboardInternal,
            DoesFilesOrFoldersExist);

        public static readonly ICommand PasteFromClipboard = new SimpleCommand<string>(
            PasteFromClipboardInternal,
            IsClipboardValid);

        public static readonly ICommand Open = new SimpleCommand<IEnumerable<string>>(
            OpenInternal,
            DoesFilesOrFoldersExist);
        public static readonly ICommand Delete = new SimpleCommand<IEnumerable<string>>(
            DeleteInternal,
            DoesFilesOrFoldersExist);
        public static readonly ICommand CreateFolder = new SimpleCommand<string>(
            CreateFolderInternal,
            DoesFileOrFolderExist);

        private static ClipboardManager _clipboardManager;

        static FileExplorerCmds()
        {
            if (Application.Current.MainWindow.IsLoaded)
            {
                Init();
            }
            else
            {
                Application.Current.MainWindow.Loaded += (_, __) =>
                {
                    Init();
                };
            }
        }

        private static void Init()
        {
            _clipboardManager = new ClipboardManager(Application.Current.MainWindow);
            _clipboardManager.ClipboardChanged += (_, __) =>
            {
                ((SimpleCommand<string>)PasteFromClipboard).RaiseCanExecuteChanged();
            };
        }

        #region Command methods
        private static void CutInternal(FileParams parameters)
        {
            if (Directory.Exists(parameters.SourcePath))
            {
                FileSystem.MoveDirectory(parameters.SourcePath, parameters.TargetPath, UIOption.AllDialogs);
            }
            else
            {
                FileSystem.MoveFile(parameters.SourcePath, parameters.TargetPath, UIOption.AllDialogs);
            }
        }

        private static void CopyInternal(FileParams parameters)
        {
            string targetFolder = Path.GetDirectoryName(parameters.TargetPath);
            string targetName = FileSystemHelper.GetValidCopyName(
                targetFolder,
                Path.GetFileName(parameters.SourcePath));

            if (Directory.Exists(parameters.SourcePath))
            {
                FileSystem.CopyDirectory(
                    parameters.SourcePath,
                    Path.Combine(targetFolder, targetName),
                    UIOption.AllDialogs);
            }
            else
            {
                FileSystem.CopyFile(
                    parameters.SourcePath,
                    Path.Combine(targetFolder, targetName),
                    UIOption.AllDialogs);
            }
        }

        private static void CopyToClipboardInternal(IEnumerable<string> paths)
        { FillClipboard(paths, EnumDropEffectPressePapier.Copy); }

        private static void CutToClipboardInternal(IEnumerable<string> paths)
        { FillClipboard(paths, EnumDropEffectPressePapier.Cut); }

        private static void FillClipboard(IEnumerable<string> paths, EnumDropEffectPressePapier type)
        {
            DataObject clipboard = new DataObject();

            StringCollection files = new StringCollection();
            foreach (string path in paths)
            {
                files.Add(path);
            }
            clipboard.SetFileDropList(files);
            SetClipboardDropEffect(clipboard, type);
            Clipboard.SetDataObject(clipboard);
        }

        private static void PasteFromClipboardInternal(string targetPath)
        {
            StringCollection files = Clipboard.GetFileDropList();
            bool isCut = IsClipboardCut();
            foreach (string file in files)
            {
                try
                {
                    if (isCut)
                        Cut.Execute(
                            new FileParams(file, Path.Combine(targetPath, Path.GetFileName(file))));
                    else
                        Copy.Execute(
                            new FileParams(file, Path.Combine(targetPath, Path.GetFileName(file))));
                }
                catch (OperationCanceledException)
                {
                }
            }
        }

        private static void OpenInternal(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", $"\"{path}\"");
                }
                else
                {
                    ProcessStartInfo pi = new ProcessStartInfo(path);
                    pi.UseShellExecute = true;
                    pi.WorkingDirectory = Path.GetDirectoryName(path);
                    pi.FileName = path;
                    pi.Verb = "OPEN";

                    try
                    {
                        Process.Start(pi);
                    }
                    catch
                    {
                        // It's possible that the file can't be opened (unknown format for example)
                        // XXX : show an error message ?
                    }
                }
            }
        }

        private static void DeleteInternal(IEnumerable<string> paths)
        {
            UIOption uiOptions = UIOption.AllDialogs;
            if (paths.Count() > 1)
            {
                uiOptions = UIOption.OnlyErrorDialogs;
                if (MessageBox.Show(
                        $"Do you really want to delete these {paths.Count()} elements definitly ?",
                        "Deleting several elements",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) !=
                    MessageBoxResult.Yes)
                    return;
            }

            foreach (string path in paths.ToList())
            {
                if (Directory.Exists(path))
                {
                    FileSystem.DeleteDirectory(
                        path,
                        uiOptions,
                        RecycleOption.SendToRecycleBin,
                        UICancelOption.DoNothing);
                }
                else
                {
                    if (File.Exists(path))
                        FileSystem.DeleteFile(
                            path,
                            uiOptions,
                            RecycleOption.SendToRecycleBin,
                            UICancelOption.DoNothing);
                }
            }
        }

        private static void CreateFolderInternal(string parentFolder)
        {
            string name = FileSystemHelper.GetValidNewFolderName(parentFolder);
            Directory.CreateDirectory(Path.Combine(parentFolder, name));
        }
        #endregion

        #region Command validations
        private static bool IsSourceTargetValid(FileParams parameters)
        {
            string targetFolder = Path.GetDirectoryName(parameters.TargetPath);
            if (!Directory.Exists(targetFolder))
                return false;
            if (parameters.TargetPath == parameters.SourcePath)
                return false;

            if (Directory.Exists(parameters.SourcePath))
            {
                if (FileSystemHelper.IsInSubfolder(targetFolder, parameters.SourcePath))
                    return false;
            }
            else if (File.Exists(parameters.SourcePath))
            {
                return true;
            }
            else
            {
                return false;
            }
            return true;
        }

        private static bool IsTargetFree(FileParams parameters)
        { return !DoesFileOrFolderExist(parameters.TargetPath); }

        private static bool DoesFilesOrFoldersExist(IEnumerable<string> paths)
        {
            if (paths == null || paths.Count() == 0)
                return false;

            foreach (string path in paths)
            {
                if (!DoesFileOrFolderExist(path))
                    return false;
            }
            return true;
        }

        private static bool DoesFileOrFolderExist(string path)
        { return !string.IsNullOrEmpty(path) && (File.Exists(path) || Directory.Exists(path)); }

        private static bool IsClipboardValid(string destinationPath)
        {
            if (!Directory.Exists(destinationPath))
                return false;
            if (!Clipboard.ContainsFileDropList())
                return false;

            try
            {
                StringCollection files = Clipboard.GetFileDropList();
                if (files.Count == 0)
                    return false;
                else
                {
                    foreach (string file in files)
                    {
                        if (!IsSourceTargetValid(new FileParams(file, destinationPath)))
                            return false;
                    }
                }
            }
            catch (ExternalException)
            {
                // 0x800401D3: CLIPBRD_E_BAD_DATA shared data may be corrupted/invalid
                // 0x800401D0: CLIPBRD_E_CANT_OPEN clipboard may be in use by another application

                return false;
            }
            return true;
        }
        #endregion

        #region Clipboard
        private static bool IsClipboardCut()
        {
            var dataDropEffect = Clipboard.GetData("Preferred DropEffect");
            if (dataDropEffect != null)
            {
                MemoryStream dropEffect = (MemoryStream)dataDropEffect;
                byte[] moveEffect = new byte[4];
                dropEffect.Read(moveEffect, 0, moveEffect.Length);
                var dragDropEffects = (DragDropEffects)BitConverter.ToInt32(moveEffect, 0);
                bool isCut = dragDropEffects.HasFlag(DragDropEffects.Move);
                return isCut;
            }

            return false;
        }

        private static void SetClipboardDropEffect(DataObject clipboard, EnumDropEffectPressePapier dropEffect)
        {
            byte[] effect;
            MemoryStream dropEffectStream = new MemoryStream();
            if (dropEffect == EnumDropEffectPressePapier.Copy)
            {
                effect = new byte[] { 5, 0, 0, 0 };
                dropEffectStream.Write(effect, 0, effect.Length);
            }
            else if (dropEffect == EnumDropEffectPressePapier.Cut)
            {
                effect = new byte[] { 2, 0, 0, 0 };
                dropEffectStream.Write(effect, 0, effect.Length);
            }

            clipboard.SetData("Preferred DropEffect", dropEffectStream);
        }
        #endregion
    }
}
