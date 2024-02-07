using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfComponents.Lib.Components.Inputs
{
    /// <summary>
    /// Either get the display member path or the ToString() of the object
    /// </summary>
    public class DisplayMemberPathConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null)
                return string.Empty;

            if (values[1] == DependencyProperty.UnsetValue)
                return values[0].ToString();

            string lDisplayMemberPath = (string)values[1];
            if (!string.IsNullOrEmpty(lDisplayMemberPath))
                return values[0].GetType().GetProperty(lDisplayMemberPath)?.GetValue(values[0]).ToString();
            else
                return values[0].ToString();
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    public class ComboBoxTags : ComboBoxSearch, INotifyPropertyChanged
    {
        public class RemoveSelectedCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly ComboBoxTags _comboBoxTags;

            public RemoveSelectedCommand(ComboBoxTags comboBoxTags)
            {
                _comboBoxTags = comboBoxTags;
            }

            public bool CanExecute(object? parameter) { return true; }

            public void Execute(object? parameter)
            {
                if (parameter == null)
                    return;

                _comboBoxTags.InternalSelectedItems.Remove(parameter);
            }
        }

        public class SelectItemCommand : ICommand
        {
            public event EventHandler? CanExecuteChanged;

            private readonly ComboBoxTags _comboBoxTags;

            public SelectItemCommand(ComboBoxTags comboBoxTags)
            {
                _comboBoxTags = comboBoxTags;
            }

            public bool CanExecute(object? parameter) { return true; }

            public void Execute(object? parameter)
            {
                if (parameter == null)
                    return;
                _comboBoxTags.AddSelectedItem();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        #region Dependency Properties
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register(
            "SelectedItems",
            typeof(IEnumerable),
            typeof(ComboBoxTags),
            new FrameworkPropertyMetadata(null, (o, e) => ((ComboBoxTags)o).OnSelectedItemsChanged()));

        public IEnumerable SelectedItems
        {
            get => (IEnumerable)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        void OnSelectedItemsChanged()
        {
            if (SelectedItems == null)
                return;

            InternalSelectedItems.Clear();
            foreach (var item in SelectedItems)
            {
                if (Items.Contains(item))
                    InternalSelectedItems.Add(item);
            }
        }
        #endregion

        #region Properties
        public ObservableCollection<object> InternalSelectedItems { get; set; } = new ObservableCollection<object>();

        public bool AllowAdd { get; set; } = false;
        public RemoveSelectedCommand RemoveSelectedCmd { get; }
        public SelectItemCommand SelectItemCmd { get; }
        #endregion

        public ComboBoxTags()
        {
            RemoveSelectedCmd = new RemoveSelectedCommand(this);
            SelectItemCmd = new SelectItemCommand(this);
            // TODO : update the selected items when the internal collection changes
            // InternalSelectedItems.CollectionChanged += InternalSelectedItems_CollectionChanged;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e) { base.OnSelectionChanged(e); }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Back && string.IsNullOrEmpty(Text) && InternalSelectedItems.Count > 0)
            {
                InternalSelectedItems.RemoveAt(InternalSelectedItems.Count - 1);
            }
            // If the user press enter, add the item to the list
            else if (e.Key == Key.Enter && SelectedItem != null)
            {
                SelectItemCmd.Execute(SelectedItem);
            }

            base.OnKeyDown(e);
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (SelectedItem == null)
                return;

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override bool DoesItemPassFilter(object value)
        {
            // If the item is already selected, don't show it in the list
            if (InternalSelectedItems.Contains(value))
                return false;

            return base.DoesItemPassFilter(value);
        }

        private void AddSelectedItem()
        {
            if (SelectedItem == null)
                return;

            InternalSelectedItems.Add(SelectedItem);

            SelectedItem = null;
        }
    }
}
