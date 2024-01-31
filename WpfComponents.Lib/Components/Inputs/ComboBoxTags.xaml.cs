using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfComponents.Lib.Inputs.Format;

namespace WpfComponents.Lib.Components.Inputs
{
    class ComboBoxTags : Control
    {
        public event EventHandler<EventArgs>? SelectionChanged;

        #region Dependency Properties
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
            "ItemsSource",
            typeof(IEnumerable),
            typeof(ComboBoxTags),
            new FrameworkPropertyMetadata(null, (o, e) => ((ComboBoxTags)o).OnItemsSourceChanged()));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        private void OnItemsSourceChanged()
        {
            throw new NotImplementedException();
        }

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

            // Check if 
        }
        #endregion

        #region Properties

        public ObservableCollection<object> InternalSelectedItems { get; set; } = new ObservableCollection<object>();

        #endregion
    }
}
