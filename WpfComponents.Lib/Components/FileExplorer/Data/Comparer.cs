using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Xml.Linq;

namespace WpfComponents.Lib.Components.FileExplorer.Data
{
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        // https://stackoverflow.com/questions/248603/natural-sort-order-in-c-sharp
        // Natural sort : permet de sort 1, 2, 10, ... au lieu de 1, 10, 2, ...
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);
    }

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
            return SafeNativeMethods.StrCmpLogicalW(nodeA.Name, nodeB.Name);
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
                return SafeNativeMethods.StrCmpLogicalW(nodeA.Name, nodeB.Name);

            return ((FileInfo)nodeA.Info).Length.CompareTo(((FileInfo)nodeB.Info).Length);
        }
    }
}
