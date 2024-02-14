using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using WpfComponents.Lib.Components.FileExplorer.Data;
using static WpfComponents.Lib.Components.FileExplorer.FileExplorerTree;

namespace WpfComponents.Lib.Components.FileExplorer.Converters
{
    // Allows adding filters on each Node without going through the viewmodel, to see how to refresh (in case of search for example)
    public class ViewFilterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable? originalCollection = values[0] as IEnumerable;

            if (originalCollection == null)
                return null;

            EnumExplorateurDisplay displayFile = (EnumExplorateurDisplay)values[1];

            CollectionViewSource collectionViewSource = new CollectionViewSource();
            collectionViewSource.Source = originalCollection;

            ICollectionView collectionView = collectionViewSource.View;
            collectionView.Filter += (obj) =>
            {
                var node = obj as ExplorerNode;

                bool display = false;
                if (displayFile.HasFlag(EnumExplorateurDisplay.Folder))
                    display = display || node is ExplorerNodeFolder;
                if (displayFile.HasFlag(EnumExplorateurDisplay.File))
                    display = display || node is ExplorerNodeFile;

                return display;
            };

            if (!displayFile.HasFlag(EnumExplorateurDisplay.NoSorting))
            {
                collectionView.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
                collectionView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }

            return collectionView;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }
}
