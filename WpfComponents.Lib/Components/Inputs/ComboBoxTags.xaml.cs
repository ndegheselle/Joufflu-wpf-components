using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace WpfComponents.Lib.Components.Inputs
{
    public class ComboBoxTags : ComboBoxSearch
    {
        public event EventHandler<EventArgs>? SelectionChanged;

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

            // Check if observable collection
        }
        #endregion

        #region Properties

        public ObservableCollection<object> InternalSelectedItems { get; set; } = new ObservableCollection<object>();

        #endregion
    }
}
