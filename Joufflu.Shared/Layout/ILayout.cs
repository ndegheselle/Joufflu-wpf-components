namespace Joufflu.Shared.Layouts
{
    public interface IPage
    {
        public ILayout? ParentLayout { get; set; }
    }

    public interface IPage<TLayout> : IPage where TLayout : ILayout
    {}

    public interface ILayout : IPage
    {
        void Hide();
        void Show(IPage page);
        // XXX : default implementation will prevent inerhiting class from having access
        void Show<TLayout>(IPage<TLayout> page) where TLayout : class, ILayout, new()
        {
            ILayout layout = new TLayout();
            layout.Show((IPage)page);
            Show(layout);
        }
    }

    public interface IDialogLayout : ILayout
    {
        public Task<bool> ShowDialog(IPage page);
        public Task<bool> ShowDialog<TLayout>(IPage<TLayout> page) where TLayout : class, ILayout, new()
        {
            ILayout layout = new TLayout();
            layout.Show((IPage)page);
            return ShowDialog(layout);
        }
        void Hide(bool result);

        void ILayout.Hide()
        {
            Hide(false);
        }

        void ILayout.Show(IPage page)
        {
            ShowDialog(page);
        }
    }
}
