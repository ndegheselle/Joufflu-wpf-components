using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace WpfComponents.Lib.Components.Inputs
{
    /// <summary>
    /// Basé sur : https://stackoverflow.com/a/41986141/10404482 Exemple :
    /// <composants:ComboBoxFiltre><composants:ComboBoxFiltre.ItemsPanel><ItemsPanelTemplate><VirtualizingStackPanel
    /// VirtualizationMode="Recycling"/></ItemsPanelTemplate></composants:ComboBoxFiltre.ItemsPanel></composants:ComboBoxFiltre>
    ///
    /// </summary>
    public class ComboBoxSearch : ComboBox
    {
        private TextBox _editableTextBox;
        private ICollectionView _collectionView;

        public bool HideFilteredItems { get; set; } = true;

        public string? FilterMemberPath { get; set; }

        public ComboBoxSearch()
        {
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
            _collectionView = new CollectionViewSource() { Source = newValue }.View;
            _collectionView.Filter += DoesItemPassFilter;
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                    IsDropDownOpen = true;
                    if (SelectedItem == null)
                        SelectedIndex = Items.Count - 1;
                    break;
                case Key.Down:
                    IsDropDownOpen = true;
                    if (SelectedItem == null)
                        SelectedIndex = 0;
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


        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (SelectedItem != null && Text == ItemGetTextFrom(SelectedItem, DisplayMemberPath))
                return;

            SelectedIndex = -1;
            if (!IsDropDownOpen)
            {
                IsDropDownOpen = true;
                // HACK : prevent the default behavior of the combobox to select all the text when the dropdown is opened
                _editableTextBox.SelectionStart = _editableTextBox.Text.Length;
            }
            RefreshFilter();
        }

        protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            // Prevent having a value that doesn't match any item (could be misleading)
            if (SelectedItem == null)
                ClearFilter();

            base.OnPreviewLostKeyboardFocus(e);
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            if (SelectedItem == null)
            {
                ClearFilter();
            }
            else if (HideFilteredItems == false)
            {
                Text = ItemGetTextFrom(SelectedItem, DisplayMemberPath);
            }

            base.OnDropDownClosed(e);
        }


        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            if (_editableTextBox == null)
                return;

            // Show italic text if no item is selected
            if (SelectedItem != null)
            {
                _editableTextBox.FontStyle = FontStyles.Normal;
                Text = ItemGetTextFrom(SelectedItem, DisplayMemberPath);
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

            _collectionView.Refresh();
            SelectFromFilter();
        }

        private void SelectFromFilter()
        {
            if (HideFilteredItems == false)
            {
                // Select closest to user input
                for (int i = 0; i < Items.Count; i++)
                {
                    if (DoesItemPassFilter(Items[i]))
                    {
                        SelectedIndex = i;
                        return;
                    }
                }
            }
            else
            {
                // Select item that matches user input exactly
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Text == ItemGetTextFrom(Items[i], FilterMemberPath))
                    {
                        SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        private void ClearFilter()
        {
            Text = string.Empty;
            RefreshFilter();
        }

        private bool DoesItemPassFilter(object value)
        {
            // If the filter is disabled, we don't filter the items
            if (HideFilteredItems == false)
                return true;

            if (value == null)
                return false;
            if (string.IsNullOrEmpty(Text))
                return true;

            return ItemGetTextFrom(value, FilterMemberPath)?.ToLower().Contains(Text.ToLower()) == true;
        }

        private string? ItemGetTextFrom(object item, string? propertyName)
        {
            if (item == null)
                return string.Empty;
            if (propertyName == null)
                return item.ToString();

            PropertyInfo? displayMemberProperty = item.GetType().GetProperty(propertyName);
            if (displayMemberProperty != null)
                return displayMemberProperty.GetValue(item)?.ToString();
            return item.ToString();
        }
    }
}