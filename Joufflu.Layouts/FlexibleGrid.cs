using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Joufflu.Layouts
{
    /// <summary>
    /// Converts between a comma-separated string and a List of GridLength values.
    /// Example: "Auto,*,2*,10" → List<GridLength> with Auto, Star, 2* Star, and 10px values
    /// </summary>
    public class GridLengthListConverter : TypeConverter
    {
        private static readonly GridLengthConverter _gridLengthConverter = new GridLengthConverter();

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        { return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType); }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            if (value is string stringValue)
            {
                if (string.IsNullOrEmpty(stringValue))
                    return new List<GridLength>();

                List<GridLength> list = [];
                foreach (string str in stringValue.Split(','))
                {
                    object? converted = _gridLengthConverter.ConvertFromString(context, culture, str.Trim());
                    if (converted is GridLength gridLength)
                        list.Add(gridLength);
                }

                return list;
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        { return destinationType == typeof(string) || base.CanConvertTo(context, destinationType); }

        public override object? ConvertTo(
            ITypeDescriptorContext? context,
            CultureInfo? culture,
            object? value,
            Type destinationType)
        {
            if (destinationType == typeof(string) && value is List<GridLength> gridLengths)
            {
                // Convert List<GridLength> back to comma-separated string
                return string.Join(
                    ",",
                    gridLengths.Select(gl => _gridLengthConverter.ConvertToString(context, culture, gl)));
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    /// <summary>
    /// Grid layout with flexible columns and rows definitions.
    /// </summary>
    public class FlexibleGrid : Panel
    {
        #region DependencyProperties
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(
            nameof(Rows),
            typeof(int?),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnLayoutPropertyChanged));
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
            nameof(Columns),
            typeof(int?),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnLayoutPropertyChanged));

        public static readonly DependencyProperty RowsHeightProperty =
            DependencyProperty.Register(
            nameof(RowsHeight),
            typeof(List<GridLength>),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnLayoutPropertyChanged));
        public static readonly DependencyProperty ColumnsWidthProperty =
            DependencyProperty.Register(
            nameof(ColumnsWidth),
            typeof(List<GridLength>),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnLayoutPropertyChanged));

        public static readonly DependencyProperty RowGapProperty =
        DependencyProperty.Register(
            nameof(RowGap),
            typeof(double),
            typeof(FlexibleGrid),
            new PropertyMetadata(0.0, OnLayoutPropertyChanged));
        public static readonly DependencyProperty ColumnGapProperty =
            DependencyProperty.Register(
            nameof(ColumnGap),
            typeof(double),
            typeof(FlexibleGrid),
            new PropertyMetadata(0.0, OnLayoutPropertyChanged));

        public static readonly DependencyProperty GapProperty =
            DependencyProperty.Register(
            nameof(Gap),
            typeof(Thickness),
            typeof(FlexibleGrid),
            new PropertyMetadata(
                new Thickness(0),
                (d, e) =>
                {
                    if (d is FlexibleGrid grid)
                    {
                        // Set row and column gap
                        var spacing = (Thickness)e.NewValue;
                        grid.SetValue(RowGapProperty, spacing.Top);
                        grid.SetValue(ColumnGapProperty, spacing.Left);
                    }
                }));
        #endregion

        #region Properties
        /// <summary>
        /// Refresh layout measure and arrangement
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FlexibleGrid grid)
            {
                grid.InvalidateMeasure();
                grid.InvalidateArrange();
            }
        }

        /// <summary>
        /// Number of rows or null to automatically calculated from the number of columns and the number of children.
        /// </summary>
        public int? Rows { get { return (int?)GetValue(RowsProperty); } set { SetValue(RowsProperty, value); } }

        /// <summary>
        /// Get the effective number of rows.
        /// </summary>
        public int EffectiveRows
        {
            get
            {
                if (Rows != null && Rows != 0)
                    return Rows.Value;
                if (Columns == null || Columns == 0)
                    return this.Children.Count;
                return (Children.Count + Columns.Value - 1) / Columns.Value;
            }
        }

        /// <summary>
        /// Number of columns or null to automatically calculated from the number of rows and the number of children.
        /// </summary>
        public int? Columns
        {
            get { return (int?)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Get the effective number of columns.
        /// </summary>
        public int EffectiveColumns
        {
            get
            {
                if (Columns != null && Columns != 0)
                    return Columns.Value;
                if (Rows == null || Rows == 0)
                    return this.Children.Count;
                return (Children.Count + Rows.Value - 1) / Rows.Value;
            }
        }

        /// <summary>
        /// Definition of the rows height. By default all rows take 1*, if the definitions is shorter than the number of
        /// rows then the definition is repeated to reach the number of rows.
        /// </summary>
        [TypeConverter(typeof(GridLengthListConverter))]
        public List<GridLength>? RowsHeight
        {
            get { return (List<GridLength>)GetValue(RowsHeightProperty); }
            set { SetValue(RowsHeightProperty, value); }
        }

        /// <summary>
        /// Definition of the rows height. By default all columns take 1*, if the definitions is shorter than the number
        /// of columns then the definition is repeated to reach the number of columns.
        /// </summary>
        [TypeConverter(typeof(GridLengthListConverter))]
        public List<GridLength>? ColumnsWidth
        {
            get { return (List<GridLength>)GetValue(ColumnsWidthProperty); }
            set { SetValue(ColumnsWidthProperty, value); }
        }

        /// <summary>
        /// Gap between rows and columns. If more that two value are provided only top and left will be used.
        /// </summary>
        public Thickness Gap { get { return (Thickness)GetValue(GapProperty); } set { SetValue(GapProperty, value); } }

        /// <summary>
        /// Gap between the rows.
        /// </summary>
        public double RowGap
        {
            get { return (double)GetValue(RowGapProperty); }
            set { SetValue(RowGapProperty, value); }
        }

        /// <summary>
        /// Gap between the columns.
        /// </summary>
        public double ColumnGap
        {
            get { return (double)GetValue(ColumnGapProperty); }
            set { SetValue(ColumnGapProperty, value); }
        }

        #endregion

        private List<GridLength> GetEffectiveRowSizes()
        {
            int effectiveRows = EffectiveRows;
            // Default definition
            if (RowsHeight == null || RowsHeight.Count <= 0)
                return Enumerable.Repeat(new GridLength(1, GridUnitType.Star), effectiveRows).ToList();

            // Definition too large
            if (RowsHeight.Count > EffectiveRows)
                return RowsHeight.Take(effectiveRows).ToList();

            // Definition too small
            if (RowsHeight.Count < EffectiveRows)
                // We repeat the know elements, for exemple 2,* will become 2,*,2,*,2 for 5 rows
                return RepeatElements(RowsHeight, effectiveRows);

            return RowsHeight;
        }

        private List<GridLength> GetEffectiveColumnSizes()
        {
            int effectiveColumns = EffectiveColumns;

            // Default definition
            if (ColumnsWidth == null || ColumnsWidth.Count <= 0)
                return Enumerable.Repeat(new GridLength(1, GridUnitType.Star), effectiveColumns).ToList();

            // Definition too large
            if (ColumnsWidth.Count > effectiveColumns)
                return ColumnsWidth.Take(effectiveColumns).ToList();

            // Definition too small
            if (ColumnsWidth.Count < effectiveColumns)
                // We repeat the know elements, for exemple 2,* will become 2,*,2,*,2 for 5 columns
                return RepeatElements(ColumnsWidth, effectiveColumns);

            return ColumnsWidth;
        }

        static List<T> RepeatElements<T>(List<T> originalList, int length)
        {
            List<T> repeatedList = new List<T>();
            int n = originalList.Count;

            for (int j = 0; j < length; j++)
            {
                repeatedList.Add(originalList[j % n]);
            }

            return repeatedList;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var rowDefs = GetEffectiveRowSizes();
            var colDefs = GetEffectiveColumnSizes();

            var rowCount = rowDefs.Count;
            var colCount = colDefs.Count;

            double fixedHeight = 0, fixedWidth = 0;
            double totalStarHeight = 0, totalStarWidth = 0;

            var autoHeights = new double[rowCount];
            var autoWidths = new double[colCount];

            // Calculate fixed sizes and accumulate star units
            for (int i = 0; i < rowCount; i++)
            {
                if (rowDefs[i].GridUnitType == GridUnitType.Pixel)
                    fixedHeight += rowDefs[i].Value;
                else if (rowDefs[i].GridUnitType == GridUnitType.Star)
                    totalStarHeight += rowDefs[i].Value;
            }

            for (int i = 0; i < colCount; i++)
            {
                if (colDefs[i].GridUnitType == GridUnitType.Pixel)
                    fixedWidth += colDefs[i].Value;
                else if (colDefs[i].GridUnitType == GridUnitType.Star)
                    totalStarWidth += colDefs[i].Value;
            }

            double rowSpacing = RowGap * Math.Max(0, rowCount - 1);
            double colSpacing = ColumnGap * Math.Max(0, colCount - 1);

            // Measure children
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                child.Measure(availableSize);

                int row = i / colCount;
                int col = i % colCount;

                if (row < rowCount && rowDefs[row].GridUnitType == GridUnitType.Auto)
                    autoHeights[row] = Math.Max(autoHeights[row], child.DesiredSize.Height);

                if (col < colCount && colDefs[col].GridUnitType == GridUnitType.Auto)
                    autoWidths[col] = Math.Max(autoWidths[col], child.DesiredSize.Width);
            }

            double autoHeight = autoHeights.Sum();
            double autoWidth = autoWidths.Sum();

            double desiredHeight = fixedHeight + autoHeight + rowSpacing;
            double desiredWidth = fixedWidth + autoWidth + colSpacing;

            return new Size(
                double.IsInfinity(availableSize.Width) ? desiredWidth : availableSize.Width,
                double.IsInfinity(availableSize.Height) ? desiredHeight : availableSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rowSizes = GetEffectiveRowSizes();
            var columnSizes = GetEffectiveColumnSizes();

            var rowHeights = new double[rowSizes.Count];
            var colWidths = new double[columnSizes.Count];
            double totalStarRow = 0, totalStarCol = 0;
            double fixedHeight = 0, fixedWidth = 0;

            // Compute fixed and star size contributions
            void ComputeSizes(IList<GridLength> sizes, double[] lengths, ref double fixedSize, ref double totalStar)
            {
                for (int i = 0; i < sizes.Count; i++)
                {
                    var size = sizes[i];
                    if (size.GridUnitType == GridUnitType.Pixel)
                    {
                        lengths[i] = size.Value;
                        fixedSize += size.Value;
                    }
                    else if (size.GridUnitType == GridUnitType.Star)
                    {
                        totalStar += size.Value;
                    }
                }
            }

            ComputeSizes(rowSizes, rowHeights, ref fixedHeight, ref totalStarRow);
            ComputeSizes(columnSizes, colWidths, ref fixedWidth, ref totalStarCol);

            // Calculate Auto sizes
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                int row = i / columnSizes.Count;
                int col = i % columnSizes.Count;

                if (rowSizes[row].GridUnitType == GridUnitType.Auto)
                    rowHeights[row] = Math.Max(rowHeights[row], child.DesiredSize.Height);

                if (columnSizes[col].GridUnitType == GridUnitType.Auto)
                    colWidths[col] = Math.Max(colWidths[col], child.DesiredSize.Width);
            }

            // Add auto sizes to fixed size totals
            fixedHeight += rowHeights.Where((_, i) => rowSizes[i].GridUnitType == GridUnitType.Auto).Sum();
            fixedWidth += colWidths.Where((_, i) => columnSizes[i].GridUnitType == GridUnitType.Auto).Sum();

            // Calculate star sizes
            double availHeight = Math.Max(0, finalSize.Height - fixedHeight - RowGap * (rowSizes.Count - 1));
            double availWidth = Math.Max(0, finalSize.Width - fixedWidth - ColumnGap * (columnSizes.Count - 1));

            void AssignStarSizes(IList<GridLength> sizes, double[] lengths, double totalStar, double available)
            {
                if (totalStar <= 0) return;
                double unit = available / totalStar;
                for (int i = 0; i < sizes.Count; i++)
                    if (sizes[i].GridUnitType == GridUnitType.Star)
                        lengths[i] = sizes[i].Value * unit;
            }

            AssignStarSizes(rowSizes, rowHeights, totalStarRow, availHeight);
            AssignStarSizes(columnSizes, colWidths, totalStarCol, availWidth);

            // Compute offsets
            double[] Offsets(double[] sizes, double gap) =>
                sizes.Select((_, i) => sizes.Take(i).Sum() + gap * i).ToArray();

            var rowOffsets = Offsets(rowHeights, RowGap);
            var colOffsets = Offsets(colWidths, ColumnGap);

            // Arrange children
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                int row = i / columnSizes.Count;
                int col = i % columnSizes.Count;

                if (row < rowHeights.Length && col < colWidths.Length)
                {
                    child.Arrange(new Rect(colOffsets[col], rowOffsets[row], colWidths[col], rowHeights[row]));
                }
            }

            return finalSize;
        }
    }
}