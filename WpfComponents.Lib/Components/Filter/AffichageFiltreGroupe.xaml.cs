using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UltraFiltre.Lib;
using static UltraFiltre.Wpf.Lib.Controles.CreationFiltres;

namespace UltraFiltre.Wpf.Lib.Controles
{
    /// <summary>
    /// Logique d'interaction pour AffichageFiltres.xaml
    /// </summary>
    public partial class AffichageFiltreGroupe : UserControl
    {
        #region DependencyProperty
        // Create a DependencyProperty for GestionDnDFiltre
        public static readonly DependencyProperty GestionDnDFiltreProperty = DependencyProperty.Register(
            "GestionDnD",
            typeof(FilterDnDHandler),
            typeof(AffichageFiltreGroupe),
            new PropertyMetadata(null));

        public FilterDnDHandler GestionDnD
        {
            get { return (FilterDnDHandler)GetValue(GestionDnDFiltreProperty); }
            set { SetValue(GestionDnDFiltreProperty, value); }
        }

        public static readonly DependencyProperty ExpressionRacineProperty = DependencyProperty.Register(
            "GroupeRacine",
            typeof(FiltreGroupe),
            typeof(AffichageFiltreGroupe),
            new PropertyMetadata(null, (o, e) => ((AffichageFiltreGroupe)o).OnRacineChanged()));

        private void OnRacineChanged()
        {
            if (GroupeRacine == null)
                return;

            if (GroupeRacine.Enfants.Count == 0)
                GroupeRacine.Enfants.Add(new Filtre() { GroupeParent = GroupeRacine });
        }

        public FiltreGroupe GroupeRacine
        {
            get { return (FiltreGroupe)GetValue(ExpressionRacineProperty); }
            set { SetValue(ExpressionRacineProperty, value); }
        }

        public static readonly DependencyProperty AffichageProperty = DependencyProperty.Register(
            "Affichage",
            typeof(EnumAffichage),
            typeof(AffichageFiltreGroupe),
            new PropertyMetadata(EnumAffichage.Simple));

        public EnumAffichage Affichage
        {
            get { return (EnumAffichage)GetValue(AffichageProperty); }
            set { SetValue(AffichageProperty, value); }
        }

        #endregion
        public List<EnumConjonctionFiltre> OperateursConjonction { get; }

        public AffichageFiltreGroupe()
        {
            InitializeComponent();
            OperateursConjonction = Enum.GetValues(typeof(EnumConjonctionFiltre)).Cast<EnumConjonctionFiltre>().ToList();
        }

        #region UI Events
        private void MenuItem_Ajouter_Click(object sender, RoutedEventArgs e)
        {
            var lPropFiltre = (Filtre)((FrameworkElement)sender).DataContext;
            var lNouveau = new Filtre();
            lNouveau.AjouterRelatifA(lPropFiltre, 1);
        }

        private void MenuItem_Supprimer_Click(object sender, RoutedEventArgs e)
        {
            var lPropFiltre = (Filtre)((FrameworkElement)sender).DataContext;
            lPropFiltre.Supprimer();
        }

        private void MenuItem_Grouper_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuItem_Degrouper_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuItem_DeplacementMonter_Click(object sender, RoutedEventArgs e)
        {
            var lPropFiltre = (Filtre)((FrameworkElement)sender).DataContext;
            lPropFiltre.Deplacer(-1);
        }

        private void MenuItem_DeplacementDescendre_Click(object sender, RoutedEventArgs e)
        {
            var lPropFiltre = (Filtre)((FrameworkElement)sender).DataContext;
            lPropFiltre.Deplacer(+1);
        }

        private void AfficherActions_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement lElement = (FrameworkElement)sender;
            var lContextMenu = (ContextMenu)lElement.FindResource("ContextMenuFiltre");
            lContextMenu.PlacementTarget = lElement;
            lContextMenu.IsOpen = true;
        }

        #endregion

        #region DnD
        protected void HandleMouseDown(object sender, MouseButtonEventArgs e)
        { GestionDnD.HandleDragMouseDown(sender, e); }

        protected void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        { GestionDnD.HandleDragMouseMove(sender, e); }

        protected void HandleDragOver(object sender, DragEventArgs e)
        {
            GestionDnD.HandleDragOver(sender, e);
            e.Handled = true;
        }

        protected void HandleDragLeave(object sender, DragEventArgs e)
        {
            GestionDnD.HandleDragLeave(sender, e);

            Border lRow = sender as Border;
            lRow.Padding = new Thickness(0);
            e.Handled = true;
        }

        protected void HandleDrop(object sender, DragEventArgs e)
        {
            GestionDnD.HandleDrop(sender, e);
            Border lRow = sender as Border;
            lRow.Padding = new Thickness(0);
            e.Handled = true;
        }

        private void DataGridRow_DragEnter(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
            Border lRow = sender as Border;
            lRow.Padding = new Thickness(0, 10, 0, 0);
            e.Handled = true;
        }
        #endregion
    }
}
