using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Joufflu.Shared
{
    /// <summary>
    /// Drag and Drop handling
    /// </summary>
    public abstract class BaseDnDHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        #region Properties

        private readonly Popup _popup;
        private readonly FrameworkElement _parentUI;

        private int _clickCount = 0;
        private Point _clickPosition = new Point();

        private bool _isDragging;
        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                if (_isDragging == value)
                    return;
                _isDragging = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Use the OS minimal distance before starting the D&D to avoid starting it by mistake
        /// If the source is a dedicated Element it's better to set it to false
        /// </summary>
        public bool UseMinimalDistance { get; set; } = true;

        #endregion

        public BaseDnDHandler(FrameworkElement parent, Popup popup)
        {
            _parentUI = parent;
            _popup = popup;
        }

        #region D&D source handling
        /// <summary>
        /// On MouseDown
        /// Ensure that the drag and drop starts with a click and get the click position
        /// </summary>
        public void HandleDragMouseDown(object sender, MouseButtonEventArgs e)
        {
            _clickCount = e.ClickCount;
            _clickPosition = e.GetPosition(_parentUI);
        }

        /// <summary>
        /// On MouseMove
        /// Start the drag and drop after the user clicked and dragged the mouse
        /// </summary>
        public void HandleDragMouseMove(object sender, MouseEventArgs e)
        {
            //  Don't start the D&D before the UI is loaded
            // -1 to be sure that the D&D starts with a MouseDown
            if (_parentUI.IsLoaded == false || _clickCount == -1)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _clickCount = -1;
                IsDragging = false;
            }

            if (UseMinimalDistance)
            {
                Point lPositionActuel = e.GetPosition(_parentUI);
                // Minimal distance to start the D&D
                if (Math.Abs(lPositionActuel.X - _clickPosition.X) < SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(lPositionActuel.Y - _clickPosition.Y) < SystemParameters.MinimumVerticalDragDistance)
                    return;
            }

            // No D&D during double click or not allowed
            if (_clickCount > 1 || CanDrag(sender, e) == false)
            {
                _clickCount = -1;
                IsDragging = false;
                return;
            }

            var destinationElement = e.OriginalSource as FrameworkElement;
            if (destinationElement == null)
                return;
            object? lDonnees = GetSourceData(destinationElement);

            if (lDonnees == null)
                return;

            UpdatePopup(lDonnees);

            try
            {
                // We reset the clickCount to avoid starting the D&D multiple times
                _clickCount = -1;
                DragDrop.DoDragDrop((DependencyObject)sender, lDonnees, DragDropEffects.Copy);
            }
            catch (ExternalException)
            {
                // DragDrop is a shared resource, it can be used by another process for example
            }
        }

        #endregion

        #region D&D destination handling

        /// <summary>
        /// On DragOver && On DragOver
        /// Display the popup and check if the drop is allowed (for the icon)
        /// </summary>
        public void HandleDragOver(object sender, DragEventArgs e)
        {
            IsDragging = true;
            if (!CanDrop(sender, e))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                HidePopup();
                return;
            }

            // D&D is valid

            var mousePosition = e.GetPosition(_parentUI);
            DisplayPopup(mousePosition);

            // HACK : DragDropEffects.Move display the same icon as None, could change it with event GiveFeedback
            e.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// On DragDrop
        /// Check if the drop is allowed and apply it
        /// </summary>
        public void HandleDrop(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
            if (e.Handled)
                return;

            IsDragging = false;
            HidePopup();
            ApplyDrop(sender, e);
        }

        /// <summary>
        /// On DragLeave
        /// Hide popup
        /// DragLeave and DragEnter may be called multiple times when moving the mouse
        /// https://stackoverflow.com/questions/5447301/wpf-drag-drop-when-does-dragleave-fire
        /// </summary>
        public void HandleDragLeave(object sender, DragEventArgs e)
        {
            IsDragging = false;
            _parentUI.Dispatcher
                .BeginInvoke(
                    new Action(
                        () =>
                        {
                            if (IsDragging == false)
                            {
                                HidePopup();
                            }
                        }));
        }

        #endregion

        #region Methods

        private void DisplayPopup(Point position)
        {
            if (_popup == null)
                return;

            _popup.IsOpen = true;
            // Need to offset the popup since the drop will try to go in the popup even with IsHitTestVisible = false
            // DnD cursor is 20px
            _popup.HorizontalOffset = position.X + 20;
            _popup.VerticalOffset = position.Y;
        }

        private void HidePopup()
        {
            if (_popup == null || _popup.IsOpen == false)
                return;
            _popup.IsOpen = false;
        }

        /// <summary>
        /// Handle special cases where the D&D is not allowed, for example the component is in a state that doesn't allow it
        /// </summary>
        protected virtual bool CanDrag(object sender, MouseEventArgs args) { return true; }

        /// <summary>
        /// Get data from the source of the D&D event
        /// </summary>
        protected virtual object? GetSourceData(FrameworkElement source)
        { return GetDataContext<object>(source); }

        protected virtual void UpdatePopup(object data)
        {
            if (_popup == null)
                return;
            _popup.DataContext = data;
        }

        /// <summary>
        /// Check if the destination is the source, if the source data is valid for the drop, ...
        /// </summary>
        protected abstract bool CanDrop(object sender, DragEventArgs args);

        /// <summary>
        /// Handle the consequences of the drop (copy files, delete, ...)
        /// </summary>
        protected abstract void ApplyDrop(object sender, DragEventArgs e);

        /// <summary>
        /// Get destination DataContext
        /// </summary>
        protected TData? GetDataContext<TData>(FrameworkElement destination) where TData : class
        { return destination?.DataContext as TData; }

        /// <summary>
        /// Get data from the dropped object
        /// </summary>
        protected TData? GetDroppedData<TData>(IDataObject dataObject) where TData : class
        {
            if (!dataObject.GetDataPresent(typeof(TData)))
                return null;
            return dataObject.GetData(typeof(TData)) as TData;
        }
        #endregion
    }

    /// <summary>
    /// Simple D&D handler for a list of data, only in the same list
    /// </summary>
    public class SimpleDnDHandler<TData> : BaseDnDHandler where TData : class
    {
        public delegate void MoveHandler(int oldIndex, int newIndex);
        public event MoveHandler? OnMove;

        private readonly IList<TData> _list;

        public SimpleDnDHandler(FrameworkElement parent, Popup popup, IList<TData> list) : base(parent, popup)
        { _list = list; }

        protected override void ApplyDrop(object sender, DragEventArgs e)
        {
            var data = GetDroppedData<TData>(e.Data);
            var destinationElement = e.OriginalSource as FrameworkElement;
            if (destinationElement == null || data == null)
                return;

            var destination = GetDataContext<TData>(destinationElement);

            int oldIndex = _list.IndexOf(data);
            _list.RemoveAt(oldIndex);
            int newIndex = -1;

            if (destination == null)
            {
                newIndex = _list.Count;
                _list.Add(data);
            }
            else
            {
                newIndex= _list.IndexOf(destination);
                _list.Insert(newIndex, data);
            }

            OnMove?.Invoke(oldIndex, newIndex);
        }

        protected override bool CanDrop(object sender, DragEventArgs args)
        {
            object? sourceData = GetDroppedData<TData>(args.Data);
            var destinationElement = args.OriginalSource as FrameworkElement;
            if (destinationElement == null)
                return false;

            object? destinationData = GetDataContext<TData>(destinationElement);
            if (sourceData == null || destinationData == null)
                return false;

            if (sourceData == destinationData)
                return false;

            return true;
        }
    }
}
