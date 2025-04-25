using Joufflu.Shared.Navigation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Usuel.Shared;

namespace Joufflu.Popups
{
    public interface IModal : ILayout
    {
        public Task<bool> Show(IModalContent page);
    }

    public interface IModalContent : IPage<Modal>
    {
        public ModalOptions Options { get; }
    }

    public class ModalOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public EnumDialogType Type { get; set; } = EnumDialogType.Info;

        public string Title { get; set; } = "";

        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ModalStackItem
    {
        public IPage Page { get; set; }
        public IModalContent? Content => Page as IModalContent;

        public TaskCompletionSource<bool>? TaskCompletion { get; set; }

        public ModalStackItem(IPage page)
        {
            Page = page;
        }
    }

    public class Modal : UserControl, IModal, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICustomCommand HideCommand { get; set; }

        public ModalStackItem? Current { get; set; }
        public ObservableCollection<ModalStackItem> Items { get; } = [];

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            HideCommand = new DelegateCommand<bool>(Hide);

            Visibility = Visibility.Collapsed;
        }

        public Task<bool> Show(IModalContent page)
        {
            Show((IPage)page);
            Current!.TaskCompletion = new TaskCompletionSource<bool>();
            return Current.TaskCompletion.Task;
        }

        public void Show(IPage page)
        {
            Current = new ModalStackItem(page);
            Items.Add(Current);
            ShowInternal(page);
        }

        private void ShowInternal(IPage page)
        {
            if (page is IPage<Modal> pageModal)
                pageModal.ParentLayout = this;
            Content = page;
            Visibility = Visibility.Visible;

        }

        public void Hide() { Hide(false); }

        public void Hide(bool result)
        {
            if (Current == null)
                return;

            Current.TaskCompletion?.SetResult(result);
            Current.Page.OnHide();
            Current = null;
            Content = null;
            Visibility = Visibility.Collapsed;
            Items.RemoveAt(Items.Count - 1);

            if (Items.Count <= 0)
                return;

            // Show previous page
            Current = Items.Last();
            ShowInternal(Current.Page);
        }
    }
}
