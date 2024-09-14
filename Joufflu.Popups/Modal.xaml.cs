using Joufflu.Shared;
using Joufflu.Shared.Navigation;
using System.ComponentModel;
using System.Windows.Controls;
using System.Xml;

namespace Joufflu.Popups
{
    public interface IModalContent : IPage<Modal>
    {
        public ModalOptions Options { get; }
    }

    public class ModalOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EnumDialogType Type { get; set; } = EnumDialogType.Info;

        public string Title { get; set; } = "";
    }

    public class Modal : UserControl, ILayout
    {
        private TaskCompletionSource<bool>? _taskCompletionSource = null;
        public IModalContent? ModalContent { get; set; }
        public ICustomCommand CloseCommand { get; set; }
        public INavigation? Navigation { get; set; }

        public Modal()
        {
            DefaultStyleKey = typeof(Modal);
            CloseCommand = new DelegateCommand(() => Close(false));
        }

        public virtual Task<bool> Show(IPage page)
        {
            Content = page;
            ModalContent = page as IModalContent;
            _taskCompletionSource = new TaskCompletionSource<bool>();
            return _taskCompletionSource.Task;
        }

        public void Close(bool result)
        {
            if (_taskCompletionSource == null)
                return;
            _taskCompletionSource.SetResult(result);
            OnClose();
        }

        public void Close()
        {
            Close(false);
        }

        protected virtual void OnClose()
        {
            _taskCompletionSource = null;
            Content = null;
            ModalContent = null;
            // FIXME : will recall Close()
            Navigation?.Close();
        }
    }

    public interface IModalValidationContent : IPage<ModalValidation>
    {
        public ModalValidationOptions Options { get; }
        public Task<bool> OnValidation();
    }

    public class ModalValidationOptions : ModalOptions
    {
        public string ValidButtonText { get; set; } = "Ok";
        public bool IsValid { get; set; } = true;
    }

    public class ModalValidation : Modal
    {
        public new IModalValidationContent? ModalContent { get; set; }
        public ICustomCommand ValidationCommand { get; set; }

        public ModalValidation()
        { ValidationCommand = new DelegateCommand(Validate); }

        private async void Validate()
        {
            if (ModalContent != null && await ModalContent.OnValidation() == false)
                return;
            Close(true);
        }

        public override Task<bool> Show(IPage page)
        {
            ModalContent = page as IModalValidationContent;
            return base.Show(page);
        }

        protected override void OnClose()
        {
            ModalContent = null;
            base.OnClose();
        }
    }
}
