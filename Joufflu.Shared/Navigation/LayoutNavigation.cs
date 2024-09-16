using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Joufflu.Shared.Navigation
{
    public interface INavigation
    {
        public void Show(IPage page);
        public void Close();
    }

    public interface IDialogNavigation : INavigation
    {
        public Task<bool> ShowDialog(IPage page);
    }

    public class LayoutNavigation : INavigation
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private ILayout? _rootLayout;
        protected ILayout? RootLayout
        {
            get => _rootLayout;
            set
            {
                if (_rootLayout == value) return;
                _rootLayout = value;

                if (_rootLayout != null)
                {
                    _rootLayout.Navigation = this;
                }
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
                _page?.OnAppearing();
            }
        }

        public IPage? CurrentPage => RootLayout ?? Page;

        public void Show(IPage page)
        {
            // Close current page
            Close(Page);

            RootLayout = ShowLayout(page as ILayoutPage);
            Page = page;
            OnPropertyChanged(nameof(CurrentPage));
        }

        public void Close()
        {
            Close(Page);
            RootLayout = null;
            Page = null;
            OnPropertyChanged(nameof(CurrentPage));
        }

        protected void Close(IPage? page)
        {
            if (page == null)
                return;

            page.OnDisappearing();

            if (page is ILayoutPage layoutPage)
            {
                Close(layoutPage.ParentLayout);
                layoutPage.OnDisappearing();
            }
        }

        protected ILayout? ShowLayout(ILayoutPage? page)
        {
            if (page == null)
                return null;

            ILayout layout = page.UseOrCreate(null);
            page.ParentLayout = layout;
            layout?.OnAppearing();

            // Show nested
            if (layout is INestedLayout nested)
                return ShowLayout(nested);
            return layout;
        }
    }

    public class DialogLayoutNavigation : LayoutNavigation
    {
        public IDialogLayout Dialog { get; set; }

        public Task<bool> ShowDialog(IPage page)
        {
            // Close current page
            Close(Page);

            ILayout? layout = ShowLayout(page as ILayoutPage);
            Page = page;
            return Dialog.Show(layout ?? Page);
        }
    }
}
