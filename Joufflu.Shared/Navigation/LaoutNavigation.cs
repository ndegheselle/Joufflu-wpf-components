using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Windows;

namespace Joufflu.Shared.Navigation
{
    public interface INavigation
    {
        public Task<bool> Show(IPage page);
        public Task<bool> Show<TLayout>(IPage<TLayout> page) where TLayout : ILayout, new();
        public void Close();
    }

    public class LayoutNavigation : INavigation, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ILayout? _pageLayout;
        protected ILayout? PageLayout
        {
            get => _pageLayout;
            set
            {
                if (_pageLayout == value) return;
                if (_pageLayout != null) _pageLayout.Close();
                _pageLayout = value;

                if (_pageLayout != null)
                    _pageLayout.Navigation = this;

                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        private IPage? _page;
        protected IPage? Page
        {
            get => _page;
            set
            {
                if (_page == value) return;
                _page = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public IPage? CurrentPage => PageLayout ?? Page;

        public void Close()
        {
            PageLayout = null;
            Page = null;
        }

        public Task<bool> Show(IPage page)
        {
            PageLayout = null;
            Page = page;
            return Task.FromResult(true);
        }

        public Task<bool> Show<TLayout>(IPage<TLayout> page) where TLayout : ILayout, new()
        {
            PageLayout = PageLayout is TLayout ? PageLayout : new TLayout();
            Page = page;
            return PageLayout.Show(page);
        }
    }
}
