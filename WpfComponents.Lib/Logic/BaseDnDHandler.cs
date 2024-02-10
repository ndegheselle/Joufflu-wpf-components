using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WpfComponents.Lib.Logic
{
    /// <summary>
    /// Gestion du Drag and Drop
    /// </summary>
    public abstract class BaseDnDHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
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
        /// Insure that the drag and drop start with a click and get the click position
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
            // Prevent D&D before the UI is loaded and after a mouse click
            if (_parentUI.IsLoaded == false || _clickCount < 0)
                return;

            if (UseMinimalDistance)
            {
                Point lPositionActuel = e.GetPosition(_parentUI);
                // Distance minimal pour déclencher un D&D
                if (Math.Abs(lPositionActuel.X - _clickPosition.X) < SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(lPositionActuel.Y - _clickPosition.Y) < SystemParameters.MinimumVerticalDragDistance)
                    return;
            }

            // Check if double click and if allowed
            if (_clickCount > 1 || CanDrag(sender, e) == false)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                object lDonnees = GetSourceData(e.OriginalSource as FrameworkElement);
                UpdatePopup(lDonnees);

                if (lDonnees == null)
                    return;

                try
                {
                    DragDrop.DoDragDrop((DependencyObject)sender, lDonnees, DragDropEffects.Copy);
                }
                catch (ExternalException)
                {
                    // DragDrop may throw an ExternalException since it's a COM object
                }
            }
            else
            {
                _clickCount = -1;
                IsDragging = false;
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

            var lPositionSouris = e.GetPosition(_parentUI);
            DisplayPopup(lPositionSouris);

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

        private void DisplayPopup(Point pPosition)
        {
            if (_popup == null)
                return;

            _popup.IsOpen = true;
            // Need to offset the popup since the drop will try to go in the popup even with IsHitTestVisible = false
            // DnD cursor is 20px
            _popup.HorizontalOffset = pPosition.X + 20;
            _popup.VerticalOffset = pPosition.Y;
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
        protected virtual bool CanDrag(object sender, MouseEventArgs pArgs) { return true; }

        /// <summary>
        /// Get data from the source of the D&D event
        /// </summary>
        protected virtual object GetSourceData(FrameworkElement lSource)
        { return GetDataContext<object>(lSource); }

        protected virtual void UpdatePopup(object lDonnees)
        {
            if (_popup == null)
                return;
            _popup.DataContext = lDonnees;
        }

        /// <summary>
        /// Check if the destination is the source, if the source data is valid for the drop, ...
        /// </summary>
        protected abstract bool CanDrop(object sender, DragEventArgs pArgs);

        /// <summary>
        /// Handle the consequences of the drop (copy files, delete, ...)
        /// </summary>
        protected abstract void ApplyDrop(object sender, DragEventArgs e);

        /// <summary>
        /// Get destination DataContext
        /// </summary>
        protected TDonnees GetDataContext<TDonnees>(FrameworkElement lDestination) where TDonnees : class
        { return lDestination?.DataContext as TDonnees; }

        /// <summary>
        /// Get data from the dropped object
        /// </summary>
        protected TDonnees GetDroppedData<TDonnees>(IDataObject pData) where TDonnees : class
        {
            if (!pData.GetDataPresent(typeof(TDonnees)))
                return null;
            return pData.GetData(typeof(TDonnees)) as TDonnees;
        }
        #endregion
    }

    /// <summary>
    /// Simple D&D handler for a list of data, only in the same list
    /// </summary>
    public class SimpleDnDHandler<TDonnees> : BaseDnDHandler where TDonnees : class
    {
        public delegate void DelacementHandler(int pAncienIndex, int pNouveauIndex);
        public event DelacementHandler OnMove;

        private readonly IList<TDonnees> _Liste;

        public SimpleDnDHandler(FrameworkElement pParent, Popup pPopup, IList<TDonnees> pListe) : base(pParent, pPopup)
        { _Liste = pListe; }

        protected override void ApplyDrop(object sender, DragEventArgs e)
        {
            var lDonnees = GetDroppedData<TDonnees>(e.Data);
            var lDestination = GetDataContext<TDonnees>(e.OriginalSource as FrameworkElement);

            int lAncienIndex = _Liste.IndexOf(lDonnees);
            _Liste.RemoveAt(lAncienIndex);
            int lNouveauIndex = _Liste.IndexOf(lDestination);

            if (lDestination == null)
                _Liste.Add(lDonnees);
            else
                _Liste.Insert(lNouveauIndex, lDonnees);

            OnMove?.Invoke(lAncienIndex, lNouveauIndex);
        }

        protected override bool CanDrop(object sender, DragEventArgs pArgs)
        {
            object lDonneesSource = GetDroppedData<TDonnees>(pArgs.Data);
            object lDonneesDestination = GetDataContext<TDonnees>(pArgs.OriginalSource as FrameworkElement);

            if (lDonneesSource == null || lDonneesDestination == null)
                return false;

            if (lDonneesSource == lDonneesDestination)
                return false;

            return true;
        }
    }
}
