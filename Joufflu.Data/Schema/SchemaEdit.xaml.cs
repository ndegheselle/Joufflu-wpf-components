using Joufflu.Data.DnD;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Joufflu.Data.Schema
{
    /// <summary>
    /// Convert a depth to a left margin for the schema properties.
    /// </summary>
    public class DepthToMarginConverter : IValueConverter
    {
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

    /// <summary>
    /// Template selector to selection an action template from a property type.
    /// Overkill for this kind of use case but nice example of how this work.
    /// </summary>
    public class ActionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? SchemaPropertyTemplate { get; set; }
        public DataTemplate? SchemaObjectTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is SchemaObjectUi)
                return SchemaObjectTemplate;
            if (item is SchemaPropertyUi)
                return SchemaPropertyTemplate;

            return base.SelectTemplate(item, container);
        }
    }

    /// <summary>
    /// Handle schema property dragging. 
    /// </summary>
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

    /// <summary>
    /// Handle schema property droping.
    /// </summary>
    public class SchemaDropHandler : DropHandler<SchemaPropertyUi>
    {
        private SchemaPropertyUi? _hoveredProperty = null;
        private readonly SchemaEdit _schema;
        public SchemaDropHandler(SchemaEdit schema)
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

            var source = GetDropData<SchemaPropertyUi>(e.Data);
            var target = ((FrameworkElement)e.OriginalSource).DataContext as SchemaPropertyUi;
            return base.IsDropAuthorized(e) && source != target;
        }

        protected override void OnPassingOver(DragEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext is not SchemaPropertyUi property)
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
        protected override void ApplyDrop(SchemaPropertyUi? data, DragEventArgs e)
        {
            if (data == null)
                return;
            if (e.OriginalSource is not FrameworkElement target)
                return;
            if (target.DataContext is not SchemaPropertyUi property)
                return;

            StopHovering();

            data.Parent?.Remove(data);
            property.Parent?.Add(data, property.Parent?.Properties.IndexOf(property) ?? 0);
            data.IsSelected = true;
        }
    }

    /// <summary>
    /// Display a data schema object and allow edit to schema property
    /// </summary>
    public partial class SchemaEdit : Control
    {
        #region Dependency properties
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(
            nameof(Root),
            typeof(SchemaObjectUi),
            typeof(SchemaEdit),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(
            nameof(IsReadOnly),
            typeof(bool),
            typeof(SchemaEdit),
            new PropertyMetadata(false));
        #endregion

        public SchemaObjectUi Root
        {
            get { return (SchemaObjectUi)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public SchemaDragHandler DragHandler { get; }
        public SchemaDropHandler DropHandler { get; }

        public SchemaEdit()
        {
            DropHandler = new SchemaDropHandler(this);
            DragHandler = new SchemaDragHandler(this, DropHandler);
        }
    }

    public partial class SchemaItem : Control
    {
        #region Dependency properties
        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.Register(
            nameof(Property),
            typeof(SchemaPropertyUi),
            typeof(SchemaItem),
            new PropertyMetadata(null));

        public static readonly DependencyProperty IsActionsVisibleProperty =
            DependencyProperty.Register(
            nameof(IsActionsVisible),
            typeof(bool),
            typeof(SchemaItem),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(
            nameof(IsEditing),
            typeof(bool),
            typeof(SchemaItem),
            new PropertyMetadata(false));
        #endregion

        public SchemaPropertyUi Property
        {
            get { return (SchemaPropertyUi)GetValue(PropertyProperty); }
            set { SetValue(PropertyProperty, value); }
        }

        public bool IsActionsVisible
        {
            get { return (bool)GetValue(IsActionsVisibleProperty); }
            set { SetValue(IsActionsVisibleProperty, value); }
        }

        public bool IsEditing
        {
            get { return (bool)GetValue(IsEditingProperty); }
            set { SetValue(IsEditingProperty, value); }
        }
    }
}