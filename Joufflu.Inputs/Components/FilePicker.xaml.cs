using Joufflu.Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Usuel.Shared;

namespace Joufflu.Inputs.Components
{
    public class FilePickerOptions
    {
        public string Filter { get; set; } = "";
        public string DefaultExtension { get; set; } = "*.*";
    }

    public class FilePicker : Control
    {
        #region Dependency Properties
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(FilePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string? FilePath
        {
            get { return (string?)GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(
            nameof(Options),
            typeof(FilePickerOptions),
            typeof(FilePicker),
            new PropertyMetadata(null));

        public FilePickerOptions? Options
        {
            get { return (FilePickerOptions?)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }
        #endregion

        public ICommand ClearCommand { get; set; }

        public ICommand SelectCommand { get; set; }

        public FilePicker()
        {
            ClearCommand = new DelegateCommand(Clear);
            SelectCommand = new DelegateCommand(Select);
        }

        private void Clear() { FilePath = ""; }

        private void Select()
        {
            Options ??= new FilePickerOptions();
            // Open file dialog
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = Options.DefaultExtension;
            dialog.Filter = Options.Filter;
            if (dialog.ShowDialog() == true)
            {
                FilePath = dialog.FileName;
            }
        }
    }
}
