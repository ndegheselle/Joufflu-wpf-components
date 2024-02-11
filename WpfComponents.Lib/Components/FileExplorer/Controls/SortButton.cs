using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace WpfComponents.Lib.Components.FileExplorer.Controls
{
    /// <summary>
    /// Allow to synchronize the different sort buttons so that only one can sort at the same time
    /// </summary>
    public interface IMediatorSortDirection
    {
        event EventHandler<ListSortDirection?> OnSortDirectionChange;
        void Notify(object sender, ListSortDirection? direction);
    }

    internal class ButtonSort : Button
    {
        public static readonly DependencyProperty MediatorProperty = DependencyProperty.Register("Mediator",
            typeof(IMediatorSortDirection), typeof(ButtonSort), new PropertyMetadata(null, (o, value) => ((ButtonSort)o).OnMediatorChange()));

        private void OnMediatorChange()
        {
            if (Mediator == null) return;
            Mediator.OnSortDirectionChange += (sender, value) =>
            {
                if (sender == this) return;
                SortDirection = null;
            };
        }
        public IMediatorSortDirection Mediator
        {
            get { return (IMediatorSortDirection)GetValue(MediatorProperty); }
            set { SetValue(MediatorProperty, value); }
        }

        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register("SortDirection",
            typeof(ListSortDirection?), typeof(ButtonSort), new PropertyMetadata(null));
        public ListSortDirection? SortDirection
        {
            get { return (ListSortDirection?)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty = DependencyProperty.Register("Target",
            typeof(string), typeof(ButtonSort), new PropertyMetadata(null));
        public string Target
        {
            get { return (string)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public void Toggle()
        {
            if (SortDirection == ListSortDirection.Descending)
            {
                SortDirection = ListSortDirection.Ascending;
            }
            else
            {
                SortDirection = ListSortDirection.Descending;
            }
        }

        public ButtonSort()
        {
            this.Click += ButtonSort_Click;
        }

        private void ButtonSort_Click(object sender, RoutedEventArgs e)
        {
            Toggle();
            Mediator.Notify(this, SortDirection);
        }
    }
}
