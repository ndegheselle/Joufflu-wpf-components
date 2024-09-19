using Joufflu.Shared.Helpers;
using Joufflu.Shared.Layouts;
using System.Windows;

namespace Joufflu.Shared.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static TLayout? GetLayout<TLayout>(this DependencyObject dp) where TLayout : DependencyObject, ILayout
        {
            return MoreVisualTreeHelper.FindParent<TLayout>(dp);
        }
    }
}
