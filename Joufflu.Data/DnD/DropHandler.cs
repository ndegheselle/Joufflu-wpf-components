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
            if (!IsDropAuthorized(e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }
            e.Effects = DragDropEffects.Copy;
            OnPassingOver(e);
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
            if (e.Handled)
                return;
            ApplyDrop(e);
        }

        /// <summary>
        /// Used to apply effect the the drop is passing over a valid element.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnPassingOver(DragEventArgs e)
        { }

        /// <summary>
        /// Validates if drop operation is allowed at the target.
        /// </summary>
        protected abstract bool IsDropAuthorized(DragEventArgs e);

        /// <summary>
        /// Executes the drop operation.
        /// </summary>
        protected abstract void ApplyDrop(DragEventArgs e);

        /// <summary>
        /// Retrieves the data from the drop operation, if it is of the expected type.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static TData? GetDropData<TData>(IDataObject data) where TData : class =>
            IsDropDataOfType<TData>(data) ? data.GetData(data.GetFormats()[0]) as TData : null;

        /// <summary>
        /// Checks if the data is of the expected type (or a derived type) for the drop operation.
        /// </summary>
        /// <typeparam name="TData"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected static bool IsDropDataOfType<TData>(IDataObject data) where TData : class
        {
            var obj = data.GetData(data.GetFormats()[0]);
            return typeof(TData).IsAssignableFrom(obj.GetType());
        }
    }

    public abstract class DropHandler<T> : DropHandler where T : class
    {
        protected override bool IsDropAuthorized(DragEventArgs e) => IsDropDataOfType<T>(e.Data);
        protected override void ApplyDrop(DragEventArgs e) => ApplyDrop(GetDropData<T>(e.Data), e);
        protected abstract void ApplyDrop(T? data, DragEventArgs e);
    }
}
