using System.Windows;
using System.Windows.Media;

namespace Joufflu.Shared.Helpers
{
    public static class MoreVisualTreeHelper
    {
        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T? parent = parentObject as T;
            return parent ?? FindParent<T>(parentObject);
        }

        public static IEnumerable<DependencyObject> GetChildren(DependencyObject pElement, bool pRecursif)
        {
            if (pElement != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(pElement); i++)
                {
                    DependencyObject lChild = VisualTreeHelper.GetChild(pElement, i);
                    if (lChild != null)
                    {
                        yield return lChild;

                        if (pRecursif)
                        {
                            foreach (DependencyObject lChildOfChild in GetChildren(lChild, true))
                                yield return lChildOfChild;
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> GetChildren<T>(DependencyObject pElement, bool pRecursif) where T : DependencyObject
        {
            IEnumerable<DependencyObject> lList = GetChildren(pElement, pRecursif);
            return lList.OfType<T>();
        }

        public static T? GetChild<T>(DependencyObject pElement, bool pRecursif) where T : DependencyObject
        {
            IEnumerable<DependencyObject> lList = GetChildren(pElement, pRecursif);
            var lReturn = lList.OfType<T>().FirstOrDefault();
            return lReturn;
        }
    }
}
