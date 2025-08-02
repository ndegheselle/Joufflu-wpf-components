using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Joufflu.Popups
{
    public interface ILoading
    {
        void Show(string message);
        void Hide();
    }

    /// <summary>
    /// Logique d'interaction pour Loading.xaml
    /// </summary>
    public partial class Loading : UserControl, ILoading
    {
        private readonly Storyboard _storyboard;

        public Loading()
        {
            this.Visibility = Visibility.Hidden;
            InitializeComponent();
            _storyboard = CreateSpinning();
        }

        private Storyboard CreateSpinning()
        {
            var storyboard = new Storyboard();
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever,
            };

            Storyboard.SetTarget(rotateAnimation, SpinnerElement);
            Storyboard.SetTargetProperty(
                rotateAnimation,
                new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));

            storyboard.Children.Add(rotateAnimation);
            return storyboard;
        }

        public void Show(string message)
        {
            this.Visibility = Visibility.Visible;
            Mouse.OverrideCursor = Cursors.Wait;
            _storyboard.Begin();
            TextElement.Text = message;
        }

        public void Hide()
        {
            _storyboard.Pause();
            this.Visibility = Visibility.Hidden;
            Mouse.OverrideCursor = null;
            TextElement.Text = "";
        }
    }
}
