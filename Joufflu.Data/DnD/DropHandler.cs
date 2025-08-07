using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Joufflu.Data.DnD
{
    /// <summary>
    /// Simplify the usage of the handler : data:DropBehavior.Handler="{Binding DropHandler}"
    /// Instead of registering and bubbling the event. 
    /// </summary>
    public static class DropBehavior
    {
        public static readonly DependencyProperty HandlerProperty =
            DependencyProperty.RegisterAttached(
                "Handler",
                typeof(DropHandler),
                typeof(DropBehavior),
                new PropertyMetadata(null, OnHandlerChanged));

        public static DropHandler GetHandler(DependencyObject obj)
            => (DropHandler)obj.GetValue(HandlerProperty);

        public static void SetHandler(DependencyObject obj, DropHandler value)
            => obj.SetValue(HandlerProperty, value);

        private static void OnHandlerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not FrameworkElement element)
                return;

            if (e.NewValue is not DropHandler handler)
                return;

            element.AllowDrop = true;
            element.DragOver += handler.HandleDragOver;
            element.DragEnter += handler.HandleDragOver;
            element.Drop += handler.HandleDrop;
        }
    }

    /// <summary>
    /// Handle object dropping.
    /// </summary>
    public abstract class DropHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void HandleDragOver(object sender, DragEventArgs e)
        {
            if (!IsDropAuthorized(e.Data))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            e.Effects = DragDropEffects.Copy;
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
            if (e.Handled)
                return;

            ApplyDrop(e.Data);
        }

        /// <summary>
        /// Validates if drop operation is allowed at the target
        /// </summary>
        protected abstract bool IsDropAuthorized(IDataObject data);

        /// <summary>
        /// Executes the drop operation
        /// </summary>
        protected abstract void ApplyDrop(IDataObject data);

        protected static TData? GetDropData<TData>(IDataObject data) where TData : class =>
            data.GetDataPresent(typeof(TData)) ? data.GetData(typeof(TData)) as TData : null;
    }

    public abstract class DropHandler<T> : DropHandler where T : class
    {
        protected override bool IsDropAuthorized(IDataObject data) => GetDropData<T>(data) != null;
        protected override void ApplyDrop(IDataObject data) => ApplyDrop(GetDropData<T>(data));

        protected abstract void ApplyDrop(T? data);
    }
}
