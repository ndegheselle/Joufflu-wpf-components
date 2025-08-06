using PropertyChanged;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Usuel.Shared;

namespace Joufflu.Inputs
{
    /// <summary>
    /// Either get the display member path or the ToString() of the object
    /// </summary>
    public class DisplayMemberPathConverter : IMultiValueConverter
    {
        public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return string.Empty;

            if (values[1] == DependencyProperty.UnsetValue)
                return values[0].ToString();

            string lDisplayMemberPath = (string)values[1];
            if (!string.IsNullOrEmpty(lDisplayMemberPath))
                return values[0].GetType().GetProperty(lDisplayMemberPath)?.GetValue(values[0])?.ToString();
            else
                return values[0].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    public class ComboBoxTags : ComboBoxSearch, INotifyPropertyChanged
    {
        #region Dependency Properties
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(ComboBoxTags),
            new FrameworkPropertyMetadata(null, (o, e) => ((ComboBoxTags)o).OnSelectedItemsChanged()));

        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }
        #endregion

        #region Properties
        public IList InternalSelectedItems { get; set; } = new ObservableCollection<object>();

        public bool AllowAdd { get; set; } = false;

        public DelegateCommand<object> RemoveSelectedCmd { get; }

        // Only add to selection then clicking or pressing enter (like combobox with IsEditable = false)
        // Sad that the combobox doesn't allow to set this behavior
        private bool _ignoreNextSelection = false;
        private Popup? _popup;
        #endregion

        public ComboBoxTags()
        {
            RemoveSelectedCmd = new DelegateCommand<object>((parameter) => InternalSelectedItems.Remove(parameter));

            SizeChanged += (s, e) =>
            {
                if (_popup == null)
                    return;

                var offset = _popup.HorizontalOffset;
                _popup.HorizontalOffset = offset + 1;
                _popup.HorizontalOffset = offset;
            };
        }

        public override void OnApplyTemplate()
        {
            _popup = (Popup)GetTemplateChild("PART_Popup");
            base.OnApplyTemplate();
        }

        void OnSelectedItemsChanged()
        {
            if (SelectedItems == null || InternalSelectedItems == SelectedItems)
                return;

            if (SelectedItems.GetType().IsGenericType &&
                SelectedItems.GetType().GetGenericTypeDefinition() == typeof(ObservableCollection<>))
            {

                InternalSelectedItems = SelectedItems;
            }
            else
            {
                InternalSelectedItems = new ObservableCollection<object>();
                foreach (var item in SelectedItems)
                {
                    if (Items.Contains(item))
                        InternalSelectedItems.Add(item);
                }
            }

            /* XXX : may want to handle binding two way with simple list with something like this : 
             if (bindingExpression?.ParentBinding.Mode == BindingMode.TwoWay ||
                bindingExpression?.ParentBinding.Mode == BindingMode.OneWayToSource)
            {
                SelectedItems.Clear();
                foreach (var item in InternalSelectedItems)
                    SelectedItems.Add(item);
                // For non ObservableCollection
                bindingExpression.UpdateSource();
            }
             */
        }

        [SuppressPropertyChangedWarnings]
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (_ignoreNextSelection)
            {
                _ignoreNextSelection = false;
                return;
            }

            AddSelectedItem();
        }

        #region UI events
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
                _ignoreNextSelection = true;

            if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && InternalSelectedItems.Count > 0)
                InternalSelectedItems.RemoveAt(InternalSelectedItems.Count - 1);

            base.OnPreviewKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && InternalSelectedItems.Count > 0)
            {
                InternalSelectedItems.RemoveAt(InternalSelectedItems.Count - 1);
            }
            else if (e.Key == Key.Enter)
            {
                if (SelectedItem != null)
                {
                    AddSelectedItem();
                }
                else if (AllowAdd && !string.IsNullOrEmpty(Text))
                {
                    InternalSelectedItems.Add(Text);
                    Text = string.Empty;
                }
            }

            base.OnKeyDown(e);
        }

        #endregion

        #region Methods
        protected override bool DoesItemPassFilter(object value)
        {
            // If the item is already selected, don't show it in the list
            if (InternalSelectedItems.Contains(value) == true)
                return false;

            return base.DoesItemPassFilter(value);
        }

        private void AddSelectedItem()
        {
            if (SelectedItem == null)
                return;
            InternalSelectedItems.Add(SelectedItem);
            Text = string.Empty;
        }
        #endregion
    }
}
