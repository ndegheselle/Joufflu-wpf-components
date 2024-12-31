namespace Joufflu.Shared.Layouts
{
    public interface IPage
    {
        public void OnHide()
        { }
    }

    public interface IPage<TLayout> : IPage where TLayout : ILayout
    {
        public TLayout? ParentLayout { get; set; }
    }

    public interface ILayout : IPage
    {
        void Hide();
        void Show(IPage page);
    }

    public interface ILayout<TLayout> : ILayout, IPage<TLayout> where TLayout : ILayout
    {}
}
