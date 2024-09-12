using System.Windows.Controls;

namespace Joufflu.Popups
{
    public interface IModalContainer
    {
        public Task<bool> Show(IModal modal);
        public void Close(IModal? modal = null);
    }

    public interface IModal
    {
        public Task<bool> Show();
        public void OnClose();
    }

    public class ModalContainer : UserControl, IModalContainer
    {
        public void Close(IModal? modal = null)
        {
            modal?.OnClose();
            Content = null;
        }

        public async Task<bool> Show(IModal modal)
        {
            Content = modal;
            bool result = await modal.Show();
            Close(modal);
            return result;
        }
    }
}
