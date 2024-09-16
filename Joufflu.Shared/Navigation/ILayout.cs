namespace Joufflu.Shared.Navigation
{
    public interface IPage
    {
        void OnAppearing() { }
        void OnDisappearing() { }
    }

    public interface ILayoutPage : IPage
    {
        public ILayout? ParentLayout { get; set; }
        public Type LayoutType { get; }
        ILayout UseOrCreate(ILayout? layout);
    }

    public interface ILayoutPage<TLayout> : ILayoutPage where TLayout : class, ILayout, new()
    {
        Type ILayoutPage.LayoutType => typeof(TLayout);
        ILayout ILayoutPage.UseOrCreate(ILayout? layout)
        {
            ParentLayout = layout ?? new TLayout();
            return ParentLayout;
        }
    }

    public interface ILayout : IPage
    {
        public INavigation? Navigation { get; set; }
    }

    public interface ILayout<TPage> : ILayout where TPage : class, IPage
    {
        public TPage? PageContent { get; set; }
    }

    public interface INestedLayout : ILayout, ILayoutPage
    {
    }

    public interface INestedLayout<TLayout> : INestedLayout, ILayoutPage<TLayout> where TLayout : class, ILayout, new()
    {
    }

    public interface IDialogLayout : ILayout
    {
        public new Task<bool> Show(IPage page);
    }

    public interface IDialogLayout<TPage> : IDialogLayout, ILayout<TPage> where TPage : class, IPage
    {
    }
}
