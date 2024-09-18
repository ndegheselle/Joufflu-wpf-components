using Joufflu.Shared;
using Joufflu.Shared.Navigation;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;

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

        public ILayout? ParentLayout { get; set; }

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            CloseCommand = new DelegateCommand(() => Hide(false));
        }

        public Task<bool> ShowDialog(IPage page)
        {
            Content = page;
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
        }

        public void Show(IPage page) { ShowDialog(page); }
        public void Hide() { Hide(false); }
    }

    public interface IModalValidationContent : IPage
    {
        public ModalValidationOptions? Options { get; }

        public Task<bool> OnValidation();
    }

    public class ModalValidationOptions : ModalOptions
    {
        public string ValidButtonText { get; set; } = "Ok";

        public bool IsValid { get; set; } = true;
    }

    public class ModalValidation : Modal, IDialogLayout
    {
        public ICustomCommand ValidationCommand { get; set; }
        public IModalValidationContent? PageContent { get; set; }

        public ModalValidation()
        {
            ValidationCommand = new DelegateCommand(Validate);
            CloseCommand = new DelegateCommand(Hide);
        }

        private async void Validate()
        {
            if (PageContent != null && await PageContent.OnValidation() == false)
                return;
            Hide(true);
        }
    }
}
