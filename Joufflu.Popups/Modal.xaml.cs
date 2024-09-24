using Joufflu.Shared.Layouts;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

    public class Modal : UserControl, IModal, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ICustomCommand CloseCommand { get; set; }
        public ICustomCommand ValidationCommand { get; set; }

        // XXX : only use Content with cast instead ?
        public IPage? CurrentPage { get; set; }
        public IModalContent? CurrentContent { get; set; }
        public IModalContentValidation? CurrentContentValidation { get; set; }
        private readonly Dictionary<IPage, TaskCompletionSource<bool>> _stack = [];

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            CloseCommand = new DelegateCommand(() => Hide(false));
            ValidationCommand = new DelegateCommand(Validate);
            Visibility = Visibility.Collapsed;
        }

        public Task<bool> Show(IModalContent page)
        {
            CurrentContent = page;
            CurrentContentValidation = page as IModalContentValidation;
            Show((IPage)page);
            var taskCompletionSource = new TaskCompletionSource<bool>();
            _stack.Add(page, taskCompletionSource);
            return taskCompletionSource.Task;
        }

        public void Show(IPage page)
        {
            if (page is IPage<Modal> pageModal)
                pageModal.ParentLayout = this;
            CurrentPage = page;
            Content = page;
            Visibility = Visibility.Visible;
        }

        public void Hide() { Hide(false); }

        public void Hide(bool result)
        {
            if (CurrentPage == null)
                return;

            // Free current page
            if (_stack.ContainsKey(CurrentPage))
            {
                _stack[CurrentPage].SetResult(result);
                _stack.Remove(CurrentPage);
            }

            // Show next page or hide if stack is empty
            if (_stack.Count > 0)
            {
                Show(_stack.Last().Key);
            }
            else
            {
                CurrentPage = null;
                CurrentContent = null;
                CurrentContentValidation = null;
                Content = null;
                Visibility = Visibility.Collapsed;
            }
        }

        private async void Validate()
        {
            if (CurrentPage is IModalContentValidation validation && await validation.OnValidation() == false)
                return;
            Hide(true);
        }
    }
}
