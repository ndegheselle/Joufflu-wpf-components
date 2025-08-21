using Joufflu.Data.DnD;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Usuel.Shared.Data;

namespace Joufflu.Data.Schema
{
    public class DepthToMarginConverter : IValueConverter
    {
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">bool, indicate wheter the result should be true or false</param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object? Convert(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (value is not uint depth)
                return null;
            return new Thickness((depth - 1) * 16, 0, 0, 0);
        }

        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            System.Globalization.CultureInfo culture)
        { throw new NotImplementedException(); }
    }

    public class SchemaDropHandler : DropHandler<SchemaProperty>
    {
        private SchemaProperty? _hoveredProperty = null;
        private readonly DataSchema _schema;
        public SchemaDropHandler(DataSchema schema)
        {
            _schema = schema;
        }

        public void StopHovering()
        {
            if (_hoveredProperty == null)
                return;
            _hoveredProperty.IsHovered = false;
            _hoveredProperty = null;
        }

        protected override bool IsDropAuthorized(DragEventArgs e)
        {
            if (_schema.IsReadOnly)
                return false;

            var source = GetDropData<SchemaProperty>(e.Data);
            var target = ((FrameworkElement)e.OriginalSource).DataContext as SchemaProperty;
            return base.IsDropAuthorized(e) && source != target;
        }

        protected override void OnPassingOver(DragEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is not SchemaProperty property)
                return;

            if (_hoveredProperty != null)
                _hoveredProperty.IsHovered = false;

            _hoveredProperty = property;
            _hoveredProperty.IsHovered = true;
        }

        /// <summary>
        /// Move the property to the target property position.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="e"></param>
        protected override void ApplyDrop(SchemaProperty? data, DragEventArgs e)
        {
            if (data == null)
                return;
            if (e.OriginalSource is not FrameworkElement target)
                return;
            if (target.DataContext is not SchemaProperty property)
                return;

            StopHovering();

            data.Parent?.Remove(data);
            property.Parent?.Add(data, property.Parent?.Properties.IndexOf(property) ?? 0);
        }
    }

    public class SchemaDragHandler : DragHandler
    {
        private readonly SchemaDropHandler _dropHandler;
        public SchemaDragHandler(FrameworkElement parent, SchemaDropHandler dropHandler) : base(parent)
        {
            _dropHandler = dropHandler;
        }

        protected override void OnDragFinished()
        {
            _dropHandler.StopHovering();
        }
    }

    public partial class DataSchema : Control
    {
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(
            nameof(Root),
            typeof(SchemaObject),
            typeof(DataSchema),
            new PropertyMetadata(null));

        public SchemaObject Root
        {
            get { return (SchemaObject)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(DataSchema),
            new PropertyMetadata(false));

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public SchemaDragHandler DragHandler { get; }
        public SchemaDropHandler DropHandler { get; }

        public DataSchema()
        {
            DropHandler = new SchemaDropHandler(this);
            DragHandler = new SchemaDragHandler(this, DropHandler);
        }
    }
}