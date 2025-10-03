using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Usuel.Shared;

namespace Joufflu.Popups
{
    public enum EnumDialogType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public interface IAlert
    {
        public void Show(EnumDialogType type, string message);
        public void Hide();

        public void Info(string message);
        public void Warning(string message);
        public void Error(string message);
        public void Success(string message);
    }

    public class AlertOptions : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EnumDialogType Type { get; set; }
        public string Message { get; set; } = "";
    }

    /// <summary>
    /// Logique d'interaction pour Alert.xaml
    /// </summary>
    public partial class Alert : Control, IAlert
    {
        public AlertOptions Options { get; set; } = new AlertOptions();

        public Storyboard ProgressStoryboard { get; set; }
        public Storyboard DisplayStoryboard { get; set; }
        public ICommand CloseCommand {  get; set; }

        public Alert()
        {
            CloseCommand = new DelegateCommand(Hide);

            // CreateValue the animations
            DisplayStoryboard = new Storyboard();
            DoubleAnimation displayAnimation = new DoubleAnimation
            {
                From = -200,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            Storyboard.SetTargetProperty(displayAnimation, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
            DisplayStoryboard.Children.Add(displayAnimation);

            ProgressStoryboard = new Storyboard();
            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = 0,
                To = 100,
                Duration = new Duration(TimeSpan.FromSeconds(5))
            };
            Storyboard.SetTargetProperty(progressAnimation, new PropertyPath("Value"));
            ProgressStoryboard.Children.Add(progressAnimation);
            ProgressStoryboard.Completed += (s, e) => Hide();
        }

        #region IAlert

        public void Show(EnumDialogType type, string message)
        {
            Options.Message = message;
            Options.Type = type;
            Visibility = Visibility.Visible;
            Animate();
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        public void Error(string message)
        {
            Show(EnumDialogType.Error, message);
        }

        public void Info(string message)
        {
            Show(EnumDialogType.Info, message);
        }

        public void Success(string message)
        {
            Show(EnumDialogType.Success, message);
        }

        public void Warning(string message)
        {
            Show(EnumDialogType.Warning, message);
        }

        #endregion


        private void Animate()
        {
            var alertContainer = (FrameworkElement)Template.FindName("AlertContainer", this);
            var progressBarTimer = (FrameworkElement)Template.FindName("ProgressBarTimer", this);

            DisplayStoryboard.Seek(TimeSpan.Zero);
            DisplayStoryboard.Begin(alertContainer);
            ProgressStoryboard.Seek(TimeSpan.Zero);
            ProgressStoryboard.Begin(progressBarTimer);
        }
    }
}
