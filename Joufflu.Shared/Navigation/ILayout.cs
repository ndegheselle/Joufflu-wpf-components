namespace Joufflu.Shared.Navigation
{
    /// <summary>
    /// Page for navigation systems
    /// </summary>
    public interface IPage
    {
        public void OnHide()
        { }
    }

    /// <summary>
    /// Page with a layout as a parent, allow the page to set data in the layout
    /// </summary>
    /// <typeparam name="TLayout"></typeparam>
    public interface IPage<TLayout> : IPage where TLayout : ILayout
    {
        public TLayout? ParentLayout { get; set; }
    }

    /// <summary>
    /// Page that can contain and display another page
    /// </summary>
    public interface ILayout : IPage
    {
        void Hide();
        void Show(IPage page);
    }

    /// <summary>
    /// Nested layout
    /// </summary>
    /// <typeparam name="TLayout"></typeparam>
    public interface ILayout<TLayout> : ILayout, IPage<TLayout> where TLayout : ILayout
    {}
}
