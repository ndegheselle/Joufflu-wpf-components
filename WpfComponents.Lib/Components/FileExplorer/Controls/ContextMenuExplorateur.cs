using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfComponents.Lib.Components.FileExplorer.Data;

namespace WpfComponents.Lib.Components.FileExplorer.Controls
{
    public class ContextMenuExplorer : ContextMenu
    {
        public ContextMenuExplorer(FileExplorerBase explorer, ExplorerNode targetNode)
        {
            this.Style = (Style)FindResource(typeof(ContextMenu));
            // Open
            if (explorer.Permissions.HasFlag(EnumPermission.AllowContextMenuOpen))
            {
                if (targetNode is ExplorerNodeFile)
                {
                    this.Items.Add(new MenuItem()
                    {
                        Header = "Open",
                        CommandParameter = explorer.SelectedPaths,
                        Command = FileExplorerCmds.Open,
                    });
                }
                this.Items.Add(new MenuItem()
                {
                    Header = "Open in Explorer",
                    // Open the folder or parent folder if file
                    CommandParameter = new List<string>()
                            {
                                (targetNode is ExplorerNodeFolder)
                                ? targetNode.FullPath
                                // Open the parent or root folder if no parent
                                : targetNode?.Parent?.FullPath ?? explorer.RootNode.Path
                            },
                    Command = FileExplorerCmds.Open
                });
            }

            if (explorer.Permissions.HasFlag(EnumPermission.AllowContextMenuActions))
            {

                this.Items.Add(new Separator());

                // Cut / Copy / Paste
                {
                    if (targetNode != explorer.RootNode)
                    {
                        this.Items.Add(new MenuItem()
                        {
                            Header = "Cut",
                            CommandParameter = explorer.SelectedPaths,
                            Command = FileExplorerCmds.CutToClipboard,
                            InputGestureText = "Ctrl+X",
                        });
                        this.Items.Add(new MenuItem()
                        {
                            Header = "Copy",
                            CommandParameter = explorer.SelectedPaths,
                            Command = FileExplorerCmds.CopyToClipboard,
                            InputGestureText = "Ctrl+C",
                        });
                    }

                    if (targetNode is ExplorerNodeFolder)
                    {
                        this.Items.Add(new MenuItem()
                        {
                            Header = "Paste",
                            CommandParameter = targetNode.FullPath,
                            Command = FileExplorerCmds.PasteFromClipboard,
                            InputGestureText = "Ctrl+V",
                        });
                    }
                }

                if (targetNode is ExplorerNodeFolder folderTargetNode)
                {
                    this.Items.Add(new Separator());

                    var newFolderMenuItem = new MenuItem()
                    {
                        Header = "New Folder",
                    };
                    newFolderMenuItem.Click += (_, __) =>
                    {
                        explorer.CreateNewFolder(folderTargetNode);
                    };
                    this.Items.Add(newFolderMenuItem);
                }

                // Delete / Rename
                if (targetNode != explorer.RootNode)
                {
                    this.Items.Add(new Separator());
                    var renameMenuItem = new MenuItem()
                    {
                        Header = "Rename",
                        InputGestureText = "F2",
                    };
                    renameMenuItem.Click += (_, __) =>
                    {
                        explorer.RenameNode(targetNode);
                    };

                    this.Items.Add(renameMenuItem);
                    this.Items.Add(new MenuItem()
                    {
                        Header = "Delete",
                        CommandParameter = explorer.SelectedPaths,
                        Command = FileExplorerCmds.Delete,
                        InputGestureText = "Delete",
                    });
                }
            }

            if (targetNode.Context != null)
            {
                TextBlock textBlock = null;
                if (!string.IsNullOrEmpty(targetNode.Context.CustomIcon))
                {
                    textBlock = new TextBlock
                    {
                        Margin = new Thickness(0, 0, 2, 0),
                        Text = targetNode.Context.CustomIcon,
                        Foreground = targetNode.Context.CustomIconColor,
                        FontFamily = (FontFamily)Application.Current.Resources["SegoeFluentIcons"],
                        FontSize = 14
                    };
                }

                this.Items.Add(new Separator());
                this.Items.Add(new MenuItem()
                {
                    Icon = textBlock,
                    Header = targetNode.Context.MenuName ?? "More Information",
                    CommandParameter = targetNode.Context.Data,
                    Command = targetNode.Context.Command,
                });
            }
        }
    }
}
