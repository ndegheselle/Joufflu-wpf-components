using PropertyChanged;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Usuel.Shared;

namespace Joufflu.Inputs
{
    /// <summary>
    /// Inspired from : https://stackoverflow.com/a/41986141/10404482 
    /// </summary>
    public class ComboBoxSearch : ComboBox, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICollectionView? SourceView { get; private set; }
        public DelegateCommand ClearCommand { get; set; }

        private TextBox? _editableTextBox;
        /// <summary>
        /// Previous text used to filter the items.
        /// </summary>
        private string? _previousRefreshText;

        public ComboBoxSearch()
        {
            ClearCommand = new DelegateCommand(() => Text = null);
            // Set default options
            IsEditable = true;
            StaysOpenOnEdit = true;
            IsTextSearchEnabled = false;
        }

        public override void OnApplyTemplate()
        {
            AddHandler(TextBoxBase.TextChangedEvent, new TextChangedEventHandler(OnTextChanged));
            _editableTextBox = (TextBox)GetTemplateChild("PART_EditableTextBox");
            _editableTextBox.FontStyle = FontStyles.Italic;

            base.OnApplyTemplate();
        }

        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            if (newValue != oldValue)
            {
                // XXX : could use new ListCollectionView((IList)newValue) to avoid conflicts on the DefaultView
                // XXX : actualy if the same list is used on two ComboBoxSearch there will be conflicts
                SourceView = CollectionViewSource.GetDefaultView(newValue);
                SourceView.Filter += DoesItemPassFilter;

                // Clean events
                if (oldValue != null)
                    CollectionViewSource.GetDefaultView(oldValue).Filter -= DoesItemPassFilter;
            }
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    if (IsDropDownOpen == false)
                    {
                        IsDropDownOpen = true;
                    }
                    else if (SelectedItem == null)
                    {
                        SelectedIndex = Items.Count - 1;
                    }
                    break;
                case Key.Down:
                    if (IsDropDownOpen == false)
                    {
                        IsDropDownOpen = true;
                    }
                    else if (SelectedItem == null)
                    {
                        SelectedIndex = 0;
                    }
                    break;
                case Key.Tab:
                case Key.Enter:
                    IsDropDownOpen = false;
                    break;
                case Key.Escape:
                    IsDropDownOpen = false;
                    SelectedItem = null;
                    break;
            }
            base.OnPreviewKeyDown(e);
        }

        [SuppressPropertyChangedWarnings]
        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem != null && Text == GetTextFromItem(SelectedItem))
                return;

            SelectedIndex = -1;
            if (IsDropDownOpen == false && !string.IsNullOrEmpty(Text))
            {
                IsDropDownOpen = true;

                // HACK : prevent the default behavior of the combobox to select all the text when the dropdown is opened
                if (_editableTextBox != null)
                    _editableTextBox.SelectionStart = _editableTextBox.Text.Length;
            }
            RefreshFilter();
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // Prevent having a value that doesn't match any item (could be misleading)
            if (SelectedItem == null)
                Text = null;
            else if (_editableTextBox != null)
                _editableTextBox.FontStyle = FontStyles.Normal;

            base.OnPreviewLostKeyboardFocus(e);
        }

        [SuppressPropertyChangedWarnings]
        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_editableTextBox == null)
                return;

            // Show italic text if no item is selected
            if (SelectedItem != null)
            {
                Text = GetTextFromItem(SelectedItem);
                _editableTextBox.FontStyle = FontStyles.Normal;
                _editableTextBox.SelectAll();
            }
            else
            {
                _editableTextBox.FontStyle = FontStyles.Italic;
            }

            e.Handled = true;
        }

        private void RefreshFilter()
        {
            if (ItemsSource == null)
                return;

            // Prevent unnecessary refresh if the text has not changed
            if (_previousRefreshText == Text)
                return;
            _previousRefreshText = Text;

            SourceView?.Refresh();
            SelectFromFilter();
        }

        private void SelectFromFilter()
        {
            if (Text == string.Empty)
                return;

            // Select item that matches user input exactly
            for (int i = 0; i < Items.Count; i++)
            {
                if (Text == GetTextFromItem(Items[i]))
                {
                    SelectedIndex = i;
                    return;
                }
            }
        }

        protected virtual bool DoesItemPassFilter(object value)
        {
            if (value == null)
                return false;
            if (string.IsNullOrEmpty(Text))
                return true;

            return DoesValueContainSearch(value);
        }

        private bool DoesValueContainSearch(object value)
        {
            return GetTextFromItem(value)?.ToLower().Contains(Text.ToLower()) == true;
        }

        private string? GetTextFromItem(object item)
        {
            if (item == null)
                return string.Empty;
            if (string.IsNullOrEmpty(DisplayMemberPath))
                return item.ToString();

            PropertyInfo? displayMemberProperty = item.GetType().GetProperty(DisplayMemberPath);
            if (displayMemberProperty != null)
                return displayMemberProperty.GetValue(item)?.ToString();
            return item.ToString();
        }
    }
}