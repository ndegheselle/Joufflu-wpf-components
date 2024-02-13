using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Controles.Layout;
using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Fonctions;
using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers;
using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers.Controles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfComponents.Lib.Components.FileExplorer.Controls;
using WpfComponents.Lib.Layout;
using static GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Controles.ExplorateurFichiersTree;

namespace GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Controles
{
    // Permet d'ajouter des filtres sur chaque Node sans passer par le viewmodel, à voir comment faire pour refresh (en cas de recherche par example)
    public class ConverterViewFilter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            IEnumerable lCollectionOrigine = values[0] as IEnumerable;

            if (lCollectionOrigine == null)
                return null;

            EnumAffichageExplorateur lAfficherFichier = (EnumAffichageExplorateur)values[1];

            CollectionViewSource lCollectionViewSource = new CollectionViewSource();
            lCollectionViewSource.Source = lCollectionOrigine;

            ICollectionView lCollectionView = lCollectionViewSource.View;
            lCollectionView.Filter += (obj) =>
            {
                var lNode = obj as NodeExplorateur;

                bool lAffichage = false;
                if (lAfficherFichier.HasFlag(EnumAffichageExplorateur.Dossier))
                    lAffichage = lAffichage || lNode is NodeExplorateurDossier;
                if (lAfficherFichier.HasFlag(EnumAffichageExplorateur.Fichier))
                    lAffichage = lAffichage || lNode is NodeExplorateurFichier;

                return lAffichage;
            };

            if (!lAfficherFichier.HasFlag(EnumAffichageExplorateur.SansTri))
            {
                lCollectionView.SortDescriptions.Add(new SortDescription("Type", ListSortDirection.Ascending));
                lCollectionView.SortDescriptions.Add(new SortDescription("Nom", ListSortDirection.Ascending));
            }

            return lCollectionView;
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, CultureInfo culture)
        { throw new NotSupportedException(); }
    }

    /// <summary>
    /// Logique d'interaction pour ExplorateurFichiers.xaml
    /// </summary>
    public partial class ExplorateurFichiersTree : BaseExplorateurFichier
    {
        [Flags]
        public enum EnumAffichageExplorateur
        {
            Rien = 1,
            Fichier = 2,
            Dossier = 4,
            SansTri = 8,
            DossierRacine = 16,
            Tout = Fichier | Dossier
        }

        #region Property Changed
        private NodeExplorateurDossier _NodeParent = null;

        public NodeExplorateurDossier NodeParent
        {
            get { return _NodeParent ?? NodeRacine; }
            set
            {
                _NodeParent = value;
                if (_NodeParent != null)
                {
                    _NodeParent.Enfants.Add(NodeRacine);
                }
                OnPropertyChanged();
            }
        }

        private NodeExplorateur _NodeSelectionne = null;

        public NodeExplorateur NodeSelectionne
        {
            get { return _NodeSelectionne; }
            set
            {
                _NodeSelectionne = value;
                // Permet de mettre a jour la vue
                if (_NodeSelectionne != null)
                    _NodeSelectionne.EstSelectionneeUnique = true;
                if (value as NodeExplorateurDossier != null && NodeSelectionne != DossierSelectionne)
                    DossierSelectionne = value as NodeExplorateurDossier;

                OnPropertyChanged();
            }
        }

        public override IEnumerable<NodeExplorateur> NodeSelectionnees
        {
            get { return new List<NodeExplorateur>() { NodeSelectionne }; }
        }
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty DossierSelectionneProperty = DependencyProperty.Register(
            "DossierSelectionne",
            typeof(NodeExplorateurDossier),
            typeof(ExplorateurFichiersTree),
            new UIPropertyMetadata(null, (o, value) => ((ExplorateurFichiersTree)o).OnDossierSelectioneChange()));

        public NodeExplorateurDossier DossierSelectionne
        {
            get { return (NodeExplorateurDossier)GetValue(DossierSelectionneProperty); }
            set { SetValue(DossierSelectionneProperty, value); }
        }

        private void OnDossierSelectioneChange()
        {
            if (DossierSelectionne != NodeSelectionne)
                NodeSelectionne = DossierSelectionne;
        }

        public static readonly DependencyProperty AffichageProperty = DependencyProperty.Register(
            "Affichage",
            typeof(EnumAffichageExplorateur),
            typeof(ExplorateurFichiersTree),
            new UIPropertyMetadata(
                EnumAffichageExplorateur.Dossier,
                (o, value) => ((ExplorateurFichiersTree)o).OnAffichageChange()));

        public EnumAffichageExplorateur Affichage
        {
            get { return (EnumAffichageExplorateur)GetValue(AffichageProperty); }
            set { SetValue(AffichageProperty, value); }
        }

        public override PopupActionDnD PopupDnD => PopupTooltipDrag;

        protected override void OnNodeRacineChange(DependencyPropertyChangedEventArgs eventArgs)
        {
            base.OnNodeRacineChange(eventArgs);
            OnAffichageChange();
        }

        #endregion

        public ExplorateurFichiersTree()
        {
            // DragDropFichier = new DragDropFichier(NodeRacine);
            InitializeComponent();
        }

        #region Events
        private void OnAffichageChange()
        {
            if (NodeRacine == null)
                return;

            if (Affichage.HasFlag(EnumAffichageExplorateur.DossierRacine))
                NodeParent = new NodeExplorateurDossier(Path.GetDirectoryName(NodeRacine.Chemin));
            else
                NodeParent = null;

            RafraichirViewNodes(NodeRacine);
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            NodeExplorateurDossier lNodeDossier = tvi.DataContext as NodeExplorateurDossier;

            if (lNodeDossier == null)
                return;
            lNodeDossier.MettreAJourEnfants();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            NodeSelectionne = e.NewValue as NodeExplorateur;
            // Notifier les autres treeview qu'il faut clear leurs sélections
            if (NodeSelectionne != null && Mediateur != null)
                Mediateur.Notifier(this, NodeSelectionnees);

            if (NodeSelectionne is NodeExplorateurDossier lDossier && lDossier.EstOuvert == false)
                lDossier.MettreAJourEnfants();
        }

        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }
        #endregion

        #region Methodes

        // Rafraichir toutes les nodes pour mettre à jour les filtres / tris
        private void RafraichirViewNodes(NodeExplorateurDossier pNodeDossier)
        {
            pNodeDossier.Rafraichir();
            foreach (var lEnfant in pNodeDossier.Enfants)
            {
                var lEnfantDossier = lEnfant as NodeExplorateurDossier;
                if (lEnfantDossier != null)
                    RafraichirViewNodes(lEnfantDossier);
            }
        }

        private TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        protected override TextBox RecupererTextBoxEdition(NodeExplorateur pNode)
        {
            // XXX : il y a de grande chance qu'il y ait un crash a textBox = null (check comment c'est gérer dans la ListView si c'est le cas)
            StretchingTreeViewItem lTreeViewItem = (StretchingTreeViewItem)TreeView.ItemContainerGenerator
                .ContainerFromItem(TreeView.SelectedItem);
            TextBox textBox = VisualTreeHelperExt.GetChildren<TextBox>(lTreeViewItem, true).FirstOrDefault();

            // Set focus on the TextBox and select all text
            if (textBox != null)
            {
                textBox.Focus();
                textBox.SelectAll();
            }
            return textBox;
        }

        public override void ClearSelection()
        {
            var lNode = TreeView.SelectedItem as NodeExplorateur;
            if (lNode == null)
                return;

            lNode.EstSelectionneeUnique = false;
        }
        #endregion

        #region Drag and drop
        private void Folder_DragOver(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);

            if (e.Handled)
                return;

            // Permet d'expand le dossier grâce au Binding
            FrameworkElement lDestination = e.OriginalSource as FrameworkElement;
            NodeExplorateurDossier lDataContext = lDestination.DataContext as NodeExplorateurDossier;

            if (lDataContext != null)
                lDataContext.EstOuvert = true;
        }
        #endregion
    }
}