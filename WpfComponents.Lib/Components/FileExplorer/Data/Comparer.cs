using System;
using System.Collections;
using System.ComponentModel;
using System.IO;

namespace WpfComponents.Lib.Components.FileExplorer.Data
{
    // TODO : should use natural sort
    class NameComparer : IComparer
    {
        public ListSortDirection Direction { get; set; }

        public NameComparer(ListSortDirection direction)
        {
            Direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            var nodeA = (Direction == ListSortDirection.Ascending) ? x as ExplorerNode : y as ExplorerNode;
            var nodeB = (Direction == ListSortDirection.Ascending) ? y as ExplorerNode : x as ExplorerNode;

            if (nodeA.Type != nodeB.Type)
                return nodeA.Type.CompareTo(nodeB.Type);
            return nodeA.Name.CompareTo(nodeB.Name);
        }
    }

    class ModifiedDateComparer : IComparer
    {
        public ListSortDirection Direction { get; set; }

        public ModifiedDateComparer(ListSortDirection direction)
        {
            Direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            var nodeA = (Direction == ListSortDirection.Ascending) ? x as ExplorerNode : y as ExplorerNode;
            var nodeB = (Direction == ListSortDirection.Ascending) ? y as ExplorerNode : x as ExplorerNode;

            if (nodeA.Type != nodeB.Type)
                return nodeA.Type.CompareTo(nodeB.Type);
            return nodeA.Info.LastWriteTime.CompareTo(nodeB.Info.LastWriteTime);
        }
    }

    class SizeComparer : IComparer
    {
        public ListSortDirection Direction { get; set; }

        public SizeComparer(ListSortDirection direction)
        {
            Direction = direction;
        }

        public int Compare(object? x, object? y)
        {
            var nodeA = (Direction == ListSortDirection.Ascending) ? x as ExplorerNode : y as ExplorerNode;
            var nodeB = (Direction == ListSortDirection.Ascending) ? y as ExplorerNode : x as ExplorerNode;

            if (nodeA.Type != nodeB.Type)
                return nodeA.Type.CompareTo(nodeB.Type);
            if (nodeA.Type == EnumExplorerNodeType.Folder)
                return nodeA.Name.CompareTo(nodeB.Name);

            return ((FileInfo)nodeA.Info).Length.CompareTo(((FileInfo)nodeB.Info).Length);
        }
    }
}
