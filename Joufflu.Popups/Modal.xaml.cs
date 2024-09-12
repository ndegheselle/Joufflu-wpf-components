using Joufflu.Shared;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Joufflu.Popups
{
    public class ModalOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public EnumDialogType Type { get; set; } = EnumDialogType.Info;

        public string Title { get; set; } = "";
    }

    public class Modal : UserControl, IModal
    {
        static Modal()
        { DefaultStyleKeyProperty.OverrideMetadata(typeof(Modal), new FrameworkPropertyMetadata(typeof(Modal))); }

        private TaskCompletionSource<bool>? _taskCompletionSource = null;

        public ModalOptions Options { get; set; }

        public ICustomCommand CloseCommand { get; set; }

        public Modal(ModalOptions options)
        {
            DefaultStyleKey = typeof(Modal);
            Options = options;
            CloseCommand = new DelegateCommand(() => Close(false));
        }

        public virtual void OnClose()
        {
        }

        public Task<bool> Show()
        {
            _taskCompletionSource = new TaskCompletionSource<bool>();
            return _taskCompletionSource.Task;
        }

        protected void Close(bool result)
        {
            if (_taskCompletionSource == null)
                return;

            _taskCompletionSource.SetResult(result);
            _taskCompletionSource = null;
        }
    }

    public class ModalValidationOptions : ModalOptions
    {
        public string ValidButtonText { get; set; } = "Ok";

        public bool IsValid { get; set; } = true;
    }

    public class ModalValidation : Modal
    {
        public new ModalValidationOptions Options => (ModalValidationOptions)base.Options;

        public ICustomCommand ValidationCommand { get; set; }

        public ModalValidation(ModalValidationOptions options) : base(options)
        { ValidationCommand = new DelegateCommand(Validate); }

        private async void Validate()
        {
            if (!await OnValidation())
                return;
            Close(true);
        }

        protected Task<bool> OnValidation() { return Task.FromResult(true); }
    }
}
