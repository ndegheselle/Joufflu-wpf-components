using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Joufflu.Inputs.Components
{
    [TemplatePart(Name = ElementTextBox, Type = typeof(FrameworkElement))]
    public class TextEditable : ContentControl
    {
        protected const string ElementTextBox = "PART_TextBox";
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextEditable), new PropertyMetadata(""));

        public string Text { get { return (string)GetValue(TextProperty); } set { SetValue(TextProperty, value); } }

        public event EventHandler? TextChanged;

        public bool IsEditing { get; private set; }

        protected TextBox? EditTextBox;
        public TextEditable() { this.MouseDoubleClick += TextEditable_MouseDoubleClick; }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            EditTextBox = Template.FindName(ElementTextBox, this) as TextBox;
            if (EditTextBox != null)
            {
                // lost focus
                EditTextBox.LostFocus += EditTextBox_LostFocus;

                // entry key
                EditTextBox.KeyDown += EditTextBox_KeyDown;
            }
        }

        private void TextEditable_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!IsEditing)
                IsEditing = true;
        }

        private void EditTextBox_LostFocus(object sender, RoutedEventArgs e) { EndEditing(); }

        private void EditTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EndEditing();
                e.Handled = true;
            }
        }

        private void EndEditing()
        {
            if (IsEditing)
            {
                IsEditing = false;
                if (EditTextBox != null && Text != EditTextBox.Text)
                {
                    Text = EditTextBox.Text;
                    TextChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
