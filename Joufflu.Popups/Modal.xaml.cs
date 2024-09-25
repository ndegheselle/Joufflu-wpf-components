using Joufflu.Shared.Layouts;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Usuel.Shared;

namespace Joufflu.Popups
{
    public interface IModal : ILayout
    {
        public Task<bool> Show(IModalContent page);
    }

    public interface IModalContent : IPage
    {
        public ModalOptions? Options { get; }
    }

    public class ModalOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EnumDialogType Type { get; set; } = EnumDialogType.Info;

        public string Title { get; set; } = "";
    }

    public interface IModalContentValidation : IModalContent
    {
        public new ModalValidationOptions? Options { get; }

        ModalOptions? IModalContent.Options => Options;

        public Task<bool> OnValidation() { return Task.FromResult(true); }
    }

    public class ModalValidationOptions : ModalOptions
    {
        public string ValidButtonText { get; set; } = "Ok";

        public bool IsValid { get; set; } = true;
    }

    public class ModalStackItem
    {
        public IPage Page { get; set; }
        public IModalContent? Content => Page as IModalContent;
        public IModalContentValidation? ContentValidation => Page as IModalContentValidation;

        public TaskCompletionSource<bool>? TaskCompletion { get; set; }

        public ModalStackItem(IPage page)
        {
            Page = page;
        }
    }

    public class Modal : UserControl, IModal, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICustomCommand CloseCommand { get; set; }
        public ICustomCommand ValidationCommand { get; set; }

        // XXX : only use Content with cast instead of CurrentPage ?
        public ModalStackItem? Current { get; set; }
        public ObservableCollection<ModalStackItem> Items { get; } = [];

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            CloseCommand = new DelegateCommand(() => Hide(false));
            ValidationCommand = new DelegateCommand(Validate);
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

        private async void Validate()
        {
            if (Current?.ContentValidation != null && await Current.ContentValidation.OnValidation() == false)
                return;
            Hide(true);
        }
    }
}
