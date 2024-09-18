namespace Joufflu.Shared.Navigation
{
    public interface IPage
    {
        public ILayout? ParentLayout { get; set; }
    }

    public interface IPage<TLayout> : IPage where TLayout : ILayout
    {
        public new TLayout? ParentLayout { get; set; }
    }

    public interface ILayout : IPage
    {
        void Show(IPage page);
        void Hide();
    }

    public interface IDialogLayout : ILayout
    {
        Task<bool> ShowDialog(IPage page);
        void Hide(bool result);
    }
}
