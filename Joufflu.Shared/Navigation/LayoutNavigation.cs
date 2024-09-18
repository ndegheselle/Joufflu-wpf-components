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

    public class BaseLayoutNavigation : INavigation
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ILayout CurrentLayout { get; set; }

        public BaseLayoutNavigation(ILayout rootLayout)
        {
            CurrentLayout = rootLayout;
        }

        public void Show(IPage page)
        {
            CurrentLayout.Show(page);
            if (page is ILayout layout)
                CurrentLayout = layout;
        }

        public void Show<TLayout>(IPage<TLayout> page) where TLayout : class, ILayout, new()
        {
            CurrentLayout = CurrentLayout is TLayout ? CurrentLayout : new TLayout();
            Show((IPage)page);
        }

        public void Close()
        {
            CurrentLayout.Hide();
        }
    }

    public class LayoutNavigation : BaseLayoutNavigation<ILayout> {
    {
        public LayoutNavigation(ILayout layout) : base(layout) 
        { }
    }

    public class DialogLayoutNavigation : BaseLayoutNavigation<IDialogLayout>, IDialogNavigation
    {
        public DialogLayoutNavigation(IDialogLayout layout) : base(layout)
        {}

        public void Close(bool result)
        {
            CurrentLayout.Hide(result);
        }

        public async Task<bool> ShowDialog(IPage page)
        {
            bool result = await CurrentLayout.ShowDialog(page);
            if (page is IDialogLayout layout)
                CurrentLayout = layout;
            return result;
        }
    }
}
