using System.Reflection;

namespace Joufflu.Shared.Navigation
{
    public interface IPage
    {
    }

    public interface ILayoutPage : IPage
    {
        public Type LayoutType { get; }
        public ILayout UseOrCreate(ILayout? layout);
    }

    public interface ILayoutPage<TLayout> : ILayoutPage where TLayout : class, ILayout, new()
    {
        public TLayout? Layout { get; set; }

        Type ILayoutPage.LayoutType => typeof(TLayout);
        ILayout ILayoutPage.UseOrCreate(ILayout? layout)
        {
            Layout = layout as TLayout ?? new TLayout();
            return Layout;
        }
    }

    public interface ILayout : IPage
    {
        public INavigation? Navigation { get; set; }
        public Task<bool> Show(IPage page);
        public void Close();
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

    public interface INestedLayout<TLayout, TPage> : INestedLayout, ILayout<TPage>, ILayoutPage<TLayout>
        where TLayout : class, ILayout, new()
        where TPage : class, IPage
    {
    }
}
