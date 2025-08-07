using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Joufflu.Data
{
    // Base class for drag operations
    public abstract class BaseDragManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly FrameworkElement _parentUI;
        private Point _clickPosition;
        private bool _hasValidClick;
        private DragAdorner? _adorner;
        private AdornerLayer? _adornerLayer;

        private bool _isDragging;
        public bool IsDragging
        {
            get => _isDragging;
            private set
            {
                if (_isDragging != value)
                {
                    _isDragging = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool WithMinimumDistance { get; set; } = true;

        protected BaseDragManager(FrameworkElement parent)
        {
            _parentUI = parent ?? throw new ArgumentNullException(nameof(parent));
            _adornerLayer = AdornerLayer.GetAdornerLayer(_parentUI);
        }

        public void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            _hasValidClick = e.LeftButton == MouseButtonState.Pressed &&
                           e.ClickCount == 1 &&
                           _parentUI.IsLoaded;

            if (_hasValidClick)
                _clickPosition = e.GetPosition(_parentUI);
        }

        public void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (!_hasValidClick || e.LeftButton != MouseButtonState.Pressed)
            {
                ResetDrag();
                return;
            }

            if (WithMinimumDistance && !HasExceededMinimumDistance(e.GetPosition(_parentUI)))
                return;

            if (!IsDragAuthorized(sender, e))
            {
                ResetDrag();
                return;
            }

            var data = GetSourceData(e.OriginalSource as FrameworkElement);
            if (data == null)
                return;

            StartDragDrop(sender, data, e);
        }

        private void ResetDrag()
        {
            _hasValidClick = false;
            IsDragging = false;
            HideAdorner();
        }

        private bool HasExceededMinimumDistance(Point currentPosition)
        {
            var deltaX = Math.Abs(currentPosition.X - _clickPosition.X);
            var deltaY = Math.Abs(currentPosition.Y - _clickPosition.Y);

            return deltaX >= SystemParameters.MinimumHorizontalDragDistance ||
                   deltaY >= SystemParameters.MinimumVerticalDragDistance;
        }

        private void StartDragDrop(object sender, object data, MouseEventArgs e)
        {
            IsDragging = true;
            ShowAdorner(data, e.GetPosition(_parentUI));
            _hasValidClick = false; // Prevent multiple drag operations

            try
            {
                var result = DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Copy);
                OnDragCompleted(result);
            }
            catch (ExternalException)
            {
                // DragDrop resource may be occupied by another process
            }
            finally
            {
                IsDragging = false;
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

        protected virtual void OnDragCompleted(DragDropEffects result)
        {
            // Override in derived classes if needed
        }

        #region Abstract Methods
        /// <summary>
        /// Determines if drag operation is authorized for the current state
        /// </summary>
        protected virtual bool IsDragAuthorized(object sender, MouseEventArgs args) => true;

        /// <summary>
        /// Extracts data from the drag source element
        /// </summary>
        protected virtual object? GetSourceData(FrameworkElement? source) =>
            GetDataContext<object>(source);

        /// <summary>
        /// Creates the visual content for the drag adorner
        /// </summary>
        protected abstract FrameworkElement? CreateAdornerContent(object data);
        #endregion

        #region Helper Methods
        /// <summary>
        /// Safely retrieves DataContext from a FrameworkElement
        /// </summary>
        protected static TData? GetDataContext<TData>(FrameworkElement? element) where TData : class =>
            element?.DataContext as TData;
        #endregion
    }

    // Base class for drop operations
    public abstract class BaseDropManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly FrameworkElement _parentUI;

        private bool _isDragOver;
        public bool IsDragOver
        {
            get => _isDragOver;
            private set
            {
                if (_isDragOver != value)
                {
                    _isDragOver = value;
                    OnPropertyChanged();
                    OnDragOverChanged(value);
                }
            }
        }

        protected BaseDropManager(FrameworkElement parent)
        {
            _parentUI = parent ?? throw new ArgumentNullException(nameof(parent));
        }

        public void HandleDragOver(object sender, DragEventArgs e)
        {
            IsDragOver = true;

            if (!IsDropAuthorized(sender, e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            e.Effects = GetDropEffects(sender, e);
            OnDragOver(sender, e);
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            // Validate drop one more time
            if (!IsDropAuthorized(sender, e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            IsDragOver = false;
            ApplyDrop(sender, e);
            OnDropCompleted(sender, e);
        }

        public void HandleDragLeave(object sender, DragEventArgs e)
        {
            // Use BeginInvoke to handle multiple DragLeave/DragEnter events
            _parentUI.Dispatcher.BeginInvoke(() =>
            {
                if (!IsMouseOverElement(_parentUI))
                {
                    IsDragOver = false;
                }
            });
        }

        public void HandleDragEnter(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        private bool IsMouseOverElement(FrameworkElement element)
        {
            var position = Mouse.GetPosition(element);
            return position.X >= 0 && position.Y >= 0 &&
                   position.X <= element.ActualWidth && position.Y <= element.ActualHeight;
        }

        protected virtual void OnDragOverChanged(bool isDragOver)
        {
            // Override in derived classes for visual feedback
        }

        protected virtual void OnDragOver(object sender, DragEventArgs e)
        {
            // Override in derived classes if needed
        }

        protected virtual void OnDropCompleted(object sender, DragEventArgs e)
        {
            // Override in derived classes if needed
        }

        protected virtual DragDropEffects GetDropEffects(object sender, DragEventArgs e)
        {
            return DragDropEffects.Copy;
        }

        #region Abstract Methods
        /// <summary>
        /// Validates if drop operation is allowed at the target
        /// </summary>
        protected abstract bool IsDropAuthorized(object sender, DragEventArgs args);

        /// <summary>
        /// Executes the drop operation
        /// </summary>
        protected abstract void ApplyDrop(object sender, DragEventArgs e);
        #endregion

        #region Helper Methods
        /// <summary>
        /// Safely extracts typed data from drag operation
        /// </summary>
        protected static TData? GetDropData<TData>(IDataObject data) where TData : class =>
            data.GetDataPresent(typeof(TData)) ? data.GetData(typeof(TData)) as TData : null;

        /// <summary>
        /// Safely retrieves DataContext from a FrameworkElement
        /// </summary>
        protected static TData? GetDataContext<TData>(FrameworkElement? element) where TData : class =>
            element?.DataContext as TData;
        #endregion
    }

    // Adorner class for drag visualization
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
}