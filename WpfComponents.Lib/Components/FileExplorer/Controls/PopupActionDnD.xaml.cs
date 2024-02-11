using System.Windows;
using System.Windows.Controls.Primitives;

namespace WpfComponents.Lib.Components.FileExplorer.Controls
{
    public partial class PopupActionDnD : Popup
    {
        public PopupActionDnD()
        {
            InitializeComponent();
        }

        public void ChangeEffect(DragDropEffects pEffet)
        {
            if (pEffet == DragDropEffects.Copy)
            {
                ActionIcon.Text = "\ue710";
                ActionText.Text = "Copier";
            }
            else if (pEffet == DragDropEffects.Move)
            {
                ActionIcon.Text = "\uf0af";
                ActionText.Text = "Déplacer";
            }
        }
    }
}
