using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace Joufflu.Shared.Navigation
{
    public interface INavigation
    {
        public Task<bool> Show(IPage page);
        public Task<bool> Show(ILayoutPage page);
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

        private Dictionary<Type, ILayout> _layouts = new Dictionary<Type, ILayout>();

        public Task<bool> Show(IPage page)
        {
            if (PageLayout != null) PageLayout.Close();
            PageLayout = null;
            Page = page;
            return Task.FromResult(true);
        }

        public Task<bool> Show(ILayoutPage page)
        {
            if (PageLayout != null) PageLayout.Close();
            return ShowInternal(page);
        }

        private async Task<bool> ShowInternal(ILayoutPage page)
        {
            ILayout layout = page.UseOrCreate(null);
            PageLayout = layout;
            bool result = await PageLayout.Show(page);
            if (PageLayout is INestedLayout nested)
            {
                result &= await ShowInternal(nested);
            }
            Page = page;
            return result;
        }
    }
}
