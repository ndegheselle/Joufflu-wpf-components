using Joufflu.Shared;
using Joufflu.Shared.Navigation;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Joufflu.Popups
{
    public interface IModalContent : ILayoutPage<Modal>
    {
        public ModalOptions? Options { get; }
    }

    public class ModalOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EnumDialogType Type { get; set; } = EnumDialogType.Info;

        public string Title { get; set; } = "";
    }

    public class Modal : UserControl, ILayout<IModalContent>
    {
        private TaskCompletionSource<bool>? _taskCompletionSource = null;

        public ICustomCommand CloseCommand { get; set; }

        public INavigation? Navigation { get; set; }

        public IModalContent? PageContent { get; set; }

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            CloseCommand = new DelegateCommand(() => Close(false));
        }

        public virtual Task<bool> Show(IPage page)
        {
            PageContent = page as IModalContent;
            Content = page;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            return _taskCompletionSource.Task;
        }

        public void Close(bool result)
        {
            if (_taskCompletionSource == null)
                return;
            _taskCompletionSource.SetResult(result);
            _taskCompletionSource = null;
            Content = null;
            PageContent = null;
            // FIXME : will recall Close()
            Navigation?.Close();
        }

        public void Close() { Close(false); }
    }

    public interface IModalValidationContent : ILayoutPage<ModalValidation> 
    {
        public ModalValidationOptions? Options { get; }
        public Task<bool> OnValidation();
    }

    public class ModalValidationOptions : ModalOptions
    {
        public string ValidButtonText { get; set; } = "Ok";

        public bool IsValid { get; set; } = true;
    }

    public class ModalValidation : UserControl, INestedLayout<Modal, IModalValidationContent>, IModalContent
    {
        public ICustomCommand CloseCommand { get; set; }
        public ICustomCommand ValidationCommand { get; set; }
        public INavigation? Navigation { 
            get => Layout?.Navigation; 
            set {
                if (Layout != null)
                    Layout.Navigation = value;
            }
        }

        public Modal? Layout { get; set; }
        public IModalValidationContent? PageContent { get; set; }
        public ModalOptions? Options => PageContent?.Options;

        public ModalValidation() { 
            ValidationCommand = new DelegateCommand(Validate);
            CloseCommand = new DelegateCommand(Close);
        }

        private async void Validate()
        {
            if (PageContent != null && await PageContent.OnValidation() == false)
                return;
            Close(true);
        }

        public Task<bool> Show(IPage page)
        {
            PageContent = page as IModalValidationContent;
            Content = page;
            return Task.FromResult(true);
        }

        public void Close() { Close(false); }

        public void Close(bool result)
        {
            Layout?.Close(result);
            Content = null;
            PageContent = null;
        }
    }
}
