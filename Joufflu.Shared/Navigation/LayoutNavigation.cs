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
        public void Close(bool result);
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

        public virtual void Close()
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
            layout.Show(page);
            layout.Navigation = this;
            layout?.OnAppearing();

            // Show nested
            if (layout is INestedLayout nested)
                return ShowLayout(nested);
            return layout;
        }
    }

    public class DialogLayoutNavigation : LayoutNavigation, IDialogNavigation
    {
        private readonly IDialogLayout _dialog;

        public DialogLayoutNavigation(IDialogLayout dialog)
        {
            _dialog = dialog;
        }

        public Task<bool> ShowDialog(IPage page)
        {
            // Close current page
            Close(Page);

            ILayout? layout = ShowLayout(page as ILayoutPage);
            Page = page;
            RootLayout = _dialog;
            OnPropertyChanged(nameof(CurrentPage));
            return _dialog.Show(layout ?? Page);
        }

        public override void Close()
        {
            Close(false);
            base.Close();
        }

        public void Close(bool result)
        {
            // Set dialog result
            _dialog.Close(result);
            // Close the interface
            base.Close();
        }
    }
}
