using System.Security.RightsManagement;
using System.Windows;

namespace Joufflu.Shared
{
    public interface IPageLifecyle<TLayout> where TLayout : ILayout
    {
        public void OnAppearing(TLayout? layout);
        public void OnDisappearing(TLayout? layout);
    }

    public interface IPage
    { }

    public interface IPage<TLayout> : IPage where TLayout : ILayout
    {
        public TLayout? ParentLayout { get; set; }
    }

    public interface ILayout : IPage
    {
        public void Show(IPage page);
        public void Hide(IPage? page);
    }

    public interface INavigation
    {
        public void Show(IPage page, bool useDefaultLayout = true);
        public void Show<TLayout>(IPage<TLayout> page) where TLayout : ILayout, new();
        public void Close();
    }

    public class LayoutNavigation : INavigation
    {
        private readonly ILayout _defaultLayout;
        private ILayout? _currentLayout;
        public ILayout ActiveLayout => _currentLayout ?? _defaultLayout;

        public IPage? CurrentPage { get; private set; }

        public LayoutNavigation(ILayout defaultLayout)
        {
            _defaultLayout = defaultLayout;
        }

        public void Close()
        {
            ActiveLayout.Hide(CurrentPage);
            CurrentPage = null;
        }

        public void Show(IPage page, bool useDefaultLayout = true)
        {
            if (useDefaultLayout || _currentLayout == null)
                _currentLayout = _defaultLayout;

            CurrentPage = page;
            ActiveLayout.Show(CurrentPage);
        }

        public void Show<TLayout>(IPage<TLayout> page) where TLayout : ILayout, new()
        {
            // Create layout if not already present
            if (_currentLayout is not TLayout)
            {
                _currentLayout = new TLayout();
                _defaultLayout.Show(_currentLayout);
            }
            page.ParentLayout = (TLayout)_currentLayout;
            Show(page, false);
        }
    }
}
