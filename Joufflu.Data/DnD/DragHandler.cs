using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Joufflu.Data.DnD
{
    /// <summary>
    /// Simplify the usage of the handler : data:DragBehavior.Handler="{Binding DragHandler}"
    /// Instead of registering and bubbling the event. 
    /// </summary>
    public static class DragBehavior
    {
        public static readonly DependencyProperty HandlerProperty =
            DependencyProperty.RegisterAttached(
                "Handler",
                typeof(DragHandler),
                typeof(DragBehavior),
                new PropertyMetadata(null, OnHandlerChanged));

        public static DragHandler GetHandler(DependencyObject obj)
            => (DragHandler)obj.GetValue(HandlerProperty);  

        public static void SetHandler(DependencyObject obj, DragHandler value)
            => obj.SetValue(HandlerProperty, value);

        private static void OnHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
                return;

            if (e.NewValue is not DragHandler handler)
                return;

            element.MouseDown += handler.HandleDragMouseDown;
            element.MouseMove += handler.HandleDragMouseMove;
        }
    }

    public class DragAdorner : Adorner
    {
        private readonly FrameworkElement _child;
        private Point _position;

        public DragAdorner(UIElement adornedElement, FrameworkElement child, Point position)
            : base(adornedElement)
        {
            _child = child ?? throw new ArgumentNullException(nameof(child));
            _position = position;

            // Make the adorner semi-transparent
            _child.Opacity = 0.7;
            IsHitTestVisible = false; // Don't interfere with drag operations
        }

        public void UpdatePosition(Point position)
        {
            _position = position;
            InvalidateVisual();
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            return _child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var rect = new Rect(_position, _child.DesiredSize);
            _child.Arrange(rect);
            return finalSize;
        }
    }

    /// <summary>
    /// Handle object draging.
    /// </summary>
    public class DragHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public bool WithMinimumDistance { get; set; } = true;

        private Point _clickPosition;
        protected Point _position;
        private bool _hasValidClick;
        protected readonly FrameworkElement _parentUI;

        public DragHandler(FrameworkElement parent)
        {
            _parentUI = parent;
        }

        public virtual void HandleDragMouseDown(object sender, MouseButtonEventArgs e)
        {
            _hasValidClick = e.LeftButton == MouseButtonState.Pressed &&
                           e.ClickCount == 1 &&
                           _parentUI.IsLoaded;

            if (_hasValidClick)
                _clickPosition = e.GetPosition(_parentUI);
        }

        public virtual void HandleDragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_hasValidClick || e.LeftButton != MouseButtonState.Pressed)
                return;

            _position = e.GetPosition(_parentUI);
            if (WithMinimumDistance && !HasExceededMinimumDistance(_position))
                return;

            var data = GetSourceData(e.OriginalSource as FrameworkElement);
            if (data == null)
                return;

            if (!IsDragAuthorized(data))
                return;

            StartDragDrop(sender, data);
        }

        private bool HasExceededMinimumDistance(Point currentPosition)
        {
            var deltaX = Math.Abs(currentPosition.X - _clickPosition.X);
            var deltaY = Math.Abs(currentPosition.Y - _clickPosition.Y);

            return deltaX >= SystemParameters.MinimumHorizontalDragDistance ||
                   deltaY >= SystemParameters.MinimumVerticalDragDistance;
        }

        protected virtual void StartDragDrop(object sender, object data)
        {
            // Prevent multiple drag operations
            _hasValidClick = false;

            try
            {
                DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Copy);
            }
            catch (ExternalException)
            {
                // DragDrop resource may be occupied by another process
            }
        }

        /// <summary>
        /// Determines if drag operation is authorized for the current state
        /// </summary>
        protected virtual bool IsDragAuthorized(object data) => true;

        /// <summary>
        /// Extracts data from the drag source element
        /// </summary>
        protected virtual object? GetSourceData(FrameworkElement? source) => source?.DataContext;

    }

    public abstract class AdornerDragHandler : DragHandler
    {
        private DragAdorner? _adorner;
        private AdornerLayer? _adornerLayer => AdornerLayer.GetAdornerLayer(_parentUI);

        public AdornerDragHandler(FrameworkElement parent) : base(parent)
        {
        }

        protected override void StartDragDrop(object sender, object data)
        {
            try
            {
                ShowAdorner(_position, data);
                base.StartDragDrop(sender, data);
            }
            finally
            {
                HideAdorner();
            }
        }

        private void ShowAdorner(Point position, object data)
        {
            if (_adornerLayer == null) return;

            HideAdorner(); // Ensure no duplicate adorners

            var adornerContent = CreateAdornerContent(data);
            if (adornerContent != null)
            {
                _adorner = new DragAdorner(_parentUI, adornerContent, position);
                _adornerLayer.Add(_adorner);
            }
        }

        private void HideAdorner()
        {
            if (_adorner == null || _adornerLayer == null)
                return;

            _adornerLayer.Remove(_adorner);
            _adorner = null;
        }

        /// <summary>
        /// Creates the visual content for the drag adorner
        /// </summary>
        protected abstract FrameworkElement? CreateAdornerContent(object data);
    }
}
