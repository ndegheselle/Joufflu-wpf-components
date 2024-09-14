namespace Joufflu.Shared.Navigation
{
    public interface IPage
    {}

    public interface IPage<out TLayout> : IPage where TLayout : ILayout
    {}

    public interface ILayout : IPage
    {
        public INavigation? Navigation { get; set; }
        public Task<bool> Show(IPage page);
        public void Close();
    }
}
