using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Usuel.Shared;

namespace Joufflu.Inputs
{
    /// <summary>
    /// Text that can be edited, can be used outside a form to indicate clearly that a value can be edited
    /// </summary>
    [TemplatePart(Name = ElementTextBox, Type = typeof(FrameworkElement))]
    public class TextEditable : ContentControl, INotifyPropertyChanged
    {
        public struct TextEditedArgs
        {
            public string Text { get; set; }
            public string OldText { get; set; }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<TextEditedArgs>? TextChanged;

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextEditable), new PropertyMetadata(""));

        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        public bool IsEditing { get; private set; }

        protected const string ElementTextBox = "PART_TextBox";
        protected TextBox? EditTextBox;

        public ICommand EditCommand { get; set; }
        public ICommand ValidateCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public TextEditable()
        {
            EditCommand = new DelegateCommand(Edit);
            ValidateCommand = new DelegateCommand(ValidateEdit);
            CancelCommand = new DelegateCommand(EndEditing);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EditTextBox = Template.FindName(ElementTextBox, this) as TextBox;
        }

        private void EndEditing()
        {
            IsEditing = false;
        }

        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Edit()
        {
            if (!IsEditing && EditTextBox != null)
            {
                EditTextBox.Text = Text;
                IsEditing = true;
            }
        }

        private void ValidateEdit()
        {
            if (IsEditing)
            {
                IsEditing = false;
                if (EditTextBox != null && Text != EditTextBox.Text)
                {
                    string oldText = Text;
                    Text = EditTextBox.Text;
                    TextChanged?.Invoke(this, new TextEditedArgs() { Text = Text, OldText = oldText });
                }
            }
        }
    }
}
