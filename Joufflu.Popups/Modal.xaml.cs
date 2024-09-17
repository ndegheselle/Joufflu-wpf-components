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

    public class Modal : UserControl, IDialogLayout<IModalContent>
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

        void ILayout.Show(IPage page)
        {
            Show(page);
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

    public class ModalValidation : UserControl, ILayout<IModalValidationContent>, IModalContent
    {
        public ICustomCommand CloseCommand { get; set; }
        public ICustomCommand ValidationCommand { get; set; }
        public INavigation? Navigation { get; set; }
        public IDialogNavigation? DialogNavigation => Navigation as IDialogNavigation;

        public Modal? Layout { get; set; }
        public IModalValidationContent? PageContent { get; set; }
        public ModalOptions? Options => PageContent?.Options;

        public ModalValidation() { 
            ValidationCommand = new DelegateCommand(Validate);
            CloseCommand = new DelegateCommand(() => Navigation?.Close());
        }

        private async void Validate()
        {
            if (PageContent != null && await PageContent.OnValidation() == false)
                return;
            DialogNavigation?.Close(true);
        }

        public void Show(IPage page)
        {
            PageContent = page as IModalValidationContent;
            Content = page;
        }
    }
}
