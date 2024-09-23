using Joufflu.Shared.Layouts;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Usuel.Shared;

namespace Joufflu.Popups
{
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

    public class Modal : UserControl, IDialogLayout
    {
        private TaskCompletionSource<bool>? _taskCompletionSource = null;
        public ICustomCommand CloseCommand { get; set; }
        public IModalContent? PageContent { get; set; }

        public ILayout? ParentLayout { get; set; }

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            CloseCommand = new DelegateCommand(() => Hide(false));
            Visibility = Visibility.Collapsed;
        }

        public virtual Task<bool> ShowDialog(IPage page)
        {
            // If already open close first
            if (_taskCompletionSource != null)
                Hide(false);

            page.ParentLayout = this;
            PageContent = page as IModalContent;
            Content = page;
            Visibility = Visibility.Visible;

            _taskCompletionSource = new TaskCompletionSource<bool>();
            return _taskCompletionSource.Task;
        }

        public void Hide(bool result)
        {
            if (_taskCompletionSource == null)
                return;
            _taskCompletionSource.SetResult(result);
            _taskCompletionSource = null;
            Content = null;
            PageContent = null;
            Visibility = Visibility.Collapsed;
        }
    }

    public interface IModalValidationContent : IPage<ModalValidation>
    {
        public ModalValidationOptions? Options { get; }
        public Task<bool> OnValidation() {
            return Task.FromResult(true);
        }
    }

    public class ModalValidationOptions : ModalOptions
    {
        public string ValidButtonText { get; set; } = "Ok";

        public bool IsValid { get; set; } = true;
    }

    public class ModalValidation : UserControl, ILayout, IModalContent
    {
        public ICustomCommand ValidationCommand { get; set; }
        public ICustomCommand CloseCommand { get; set; }
        public IModalValidationContent? PageContent { get; set; }
        public ModalOptions? Options => PageContent?.Options;
        public ILayout? ParentLayout { get; set; }

        public ModalValidation()
        {
            ValidationCommand = new DelegateCommand(Validate);
            CloseCommand = new DelegateCommand(Hide);
        }

        private async void Validate()
        {
            if (PageContent != null && await PageContent.OnValidation() == false)
                return;
            ((IDialogLayout)ParentLayout!).Hide(true);
        }

        public void Hide()
        {
            Content = null;
            PageContent = null;
            ParentLayout?.Hide();
        }

        public void Show(IPage page)
        {
            PageContent = page as IModalValidationContent;
            Content = page;
        }
    }
}
