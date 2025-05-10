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
    public class FlexibleGrid : Grid
    {
        #region DependencyProperties
        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(
            nameof(Rows),
            typeof(int?),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnRowsChanged));
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
            nameof(Columns),
            typeof(int?),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnColumnsChanged));

        public static readonly DependencyProperty RowsHeightProperty =
            DependencyProperty.Register(
            nameof(RowsHeight),
            typeof(List<GridLength>),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnRowsWidthChanged));
        public static readonly DependencyProperty ColumnsWidthProperty =
            DependencyProperty.Register(
            nameof(ColumnsWidth),
            typeof(List<GridLength>),
            typeof(FlexibleGrid),
            new PropertyMetadata(null, OnColumnsWidthChanged));

        public static readonly DependencyProperty RowGapProperty =
        DependencyProperty.Register(
            nameof(RowGap),
            typeof(double),
            typeof(FlexibleGrid),
            new PropertyMetadata(0.0, OnGapChanged));
        public static readonly DependencyProperty ColumnGapProperty =
            DependencyProperty.Register(
            nameof(ColumnGap),
            typeof(double),
            typeof(FlexibleGrid),
            new PropertyMetadata(0.0, OnGapChanged));

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
                    return this.VisualChildrenCount;
                return (VisualChildrenCount + Columns.Value - 1) / Columns.Value;
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
                    return this.VisualChildrenCount;
                return (VisualChildrenCount + Rows.Value - 1) / Rows.Value;
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

        #region On changed

        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (FlexibleGrid)d;
            grid.AssignChildrenPositions();
        }

        private static void OnRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (FlexibleGrid)d;
            grid.AssignChildrenPositions();
        }

        private static void OnRowsWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (FlexibleGrid)d;
            grid.UpdateRowDefinitions();
        }

        private static void OnColumnsWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (FlexibleGrid)d;
            grid.UpdateColumnDefinitions();
        }

        private static void OnGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = (FlexibleGrid)d;
            grid.UpdateColumnDefinitions();
            grid.UpdateRowDefinitions();
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            AssignChildrenPositions();
        }

        #endregion

        private void UpdateColumnDefinitions()
        {
            if (this.VisualChildrenCount == 0)
                return;

            ColumnDefinitions.Clear();
            var effectiveColumnSizes = GetEffectiveColumnSizes();
            for (int i = 0; i < EffectiveColumns; i++)
            {
                GridLength width = effectiveColumnSizes[i];
                ColumnDefinitions.Add(new ColumnDefinition { Width = width });
                if (i < EffectiveColumns - 1)
                {
                    ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(Gap.Left) });
                }
            }
        }

        private void UpdateRowDefinitions()
        {
            if (this.VisualChildrenCount == 0)
                return;

            RowDefinitions.Clear();
            var effectiveRowSizes = GetEffectiveRowSizes();
            for (int i = 0; i < EffectiveRows; i++)
            {
                GridLength height = effectiveRowSizes[i];
                RowDefinitions.Add(new RowDefinition { Height = height });
                if (i < EffectiveRows - 1)
                {
                    RowDefinitions.Add(new RowDefinition { Height = new GridLength(Gap.Top) });
                }
            }
        }

        private void AssignChildrenPositions()
        {
            int row = 0;
            int col = 0;
            foreach (UIElement child in Children)
            {
                SetRow(child, row * 2);
                SetColumn(child, col * 2);

                col++;
                if (col >= EffectiveColumns)
                {
                    col = 0;
                    row++;
                    if (row >= EffectiveRows)
                    {
                        row = 0;
                    }
                }
            }

            // Update the grid definitions based on the current number of children
            UpdateColumnDefinitions();
            UpdateRowDefinitions();
        }
    }
}