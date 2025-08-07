using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

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

    /// <summary>
    /// Handle object draging.
    /// </summary>
    public class DragHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly FrameworkElement _parentUI;
        private Point _clickPosition;
        private bool _hasValidClick;

        public bool WithMinimumDistance { get; set; } = true;

        public DragHandler(FrameworkElement parent)
        {
            _parentUI = parent;
        }

        public void HandleDragMouseDown(object sender, MouseButtonEventArgs e)
        {
            _hasValidClick = e.LeftButton == MouseButtonState.Pressed &&
                           e.ClickCount == 1 &&
                           _parentUI.IsLoaded;

            if (_hasValidClick)
                _clickPosition = e.GetPosition(_parentUI);
        }

        public void HandleDragMouseMove(object sender, MouseEventArgs e)
        {
            if (!_hasValidClick || e.LeftButton != MouseButtonState.Pressed)
                return;

            if (WithMinimumDistance && !HasExceededMinimumDistance(e.GetPosition(_parentUI)))
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

        private void StartDragDrop(object sender, object data)
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
            finally
            {
                HideAdorner();
            }
        }

        private void ShowAdorner(object data, Point position)
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
            if (_adorner != null && _adornerLayer != null)
            {
                _adornerLayer.Remove(_adorner);
                _adorner = null;
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

}
