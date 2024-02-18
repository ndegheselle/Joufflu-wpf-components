using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using UltraFiltre.Lib;
using System;
using WpfComponents.Lib.Logic;

namespace UltraFiltre.Wpf.Lib.Controles
{
    // XXX : D&D désactivé pour l'instant, il faudrait ajouter la notion de groupe pour que ça fasse du sens
    public class FilterDnDHandler : BaseDnDHandler
    {
        public FilterDnDHandler(CreationFiltres pParent, Popup pPopup) : base(pParent, pPopup)
        {
        }

        protected override void ApplyDrop(object sender, DragEventArgs e)
        {
            var lDonneesDrop = GetDroppedData<Filtre>(e.Data);
            var lDonneesDest = GetDataContext<Filtre>((FrameworkElement)e.OriginalSource);

            var lParentActuel = lDonneesDrop.GroupeParent;

            lDonneesDrop.Supprimer();
            lDonneesDrop.AjouterRelatifA(lDonneesDest);
        }

        protected override bool CanDrop(object sender, DragEventArgs pArgs)
        {
            object lDonneesSource = GetDroppedData<Filtre>(pArgs.Data);
            object lDonneesDestination = GetDataContext<Filtre>((FrameworkElement)pArgs.OriginalSource);

            if (lDonneesSource == null || lDonneesDestination == null)
                return false;

            if (lDonneesSource == lDonneesDestination)
                return false;
            return true;
        }
    }

    /// <summary>
    /// Gestion global de l'affichage des filtres
    /// </summary>
    public partial class CreationFiltres : UserControl
    {
        // Définit l'affichage des options de groupement et de conjonction
        public enum EnumAffichage
        {
            Simple,
            Complet
        }

        public static readonly DependencyProperty GroupeRacineProperty = DependencyProperty.Register(
            "GroupeRacine",
            typeof(FiltreGroupe),
            typeof(CreationFiltres),
            new PropertyMetadata(null, (o, e) => ((CreationFiltres)o).OnExpressionRacineChanged()));

        public FiltreGroupe GroupeRacine
        {
            get { return (FiltreGroupe)GetValue(GroupeRacineProperty); }
            set { SetValue(GroupeRacineProperty, value); }
        }

        void OnExpressionRacineChanged()
        {
            if (GroupeRacine == null)
                return;

            // Appliquer le type cible
            if (TypeCible != null)
                GroupeRacine.RecuperProps(TypeCible);
        }

        public static readonly DependencyProperty AffichageProperty = DependencyProperty.Register(
            "Affichage",
            typeof(EnumAffichage),
            typeof(CreationFiltres),
            new PropertyMetadata(EnumAffichage.Complet));

        public EnumAffichage Affichage
        {
            get { return (EnumAffichage)GetValue(AffichageProperty); }
            set { SetValue(AffichageProperty, value); }
        }

        private Type _typeCible;
        public Type TypeCible
        {
            get
            {
                return _typeCible;
            }
            set
            {
                _typeCible = value;
                if (GroupeRacine == null)
                    GroupeRacine = new FiltreGroupe(null, value);
                else
                    GroupeRacine.RecuperProps(value);
            }
        }

        public FilterDnDHandler GestionDnD { get; }

        #region Init
        public CreationFiltres()
        {
            InitializeComponent();
            GestionDnD = new FilterDnDHandler(this, this.PopupDnd) { UseMinimalDistance = false };
        }
        #endregion

        public Expression<Func<TTypeCible, bool>> GetExpression<TTypeCible>()
        { return GestionFiltres.GetExpression<TTypeCible>(GroupeRacine.Enfants); }


        /// <summary>
        /// Permet de filtrer automatiquement un ItemsControl (DataGrid, ListBox, ComboBox ..ect). <typeparam
        /// name="T">Type de l'objet qui a été bindé dans l'ItemsControl.</typeparam>
        /// </summary>
        public void Filter<T>(ItemsControl pDataGrid) where T : class => Filter<T>(pDataGrid.ItemsSource);

        /// <summary>
        /// Permet de filtrer automatiquement l'IEnumerable bindé sur un Control. <typeparam name="T">Type de l'objet
        /// qui a été bindé dans l'ItemsControl.</typeparam>
        /// </summary>
        public void Filter<T>(IEnumerable pEnumerable) where T : class
        {
            var lExpression = GetExpression<T>();
            var lFunc = lExpression.Compile();

            ICollectionView lView = CollectionViewSource.GetDefaultView(pEnumerable) ??
                throw new NullReferenceException("Impossible de récupérer la CollectionViewSource de la DataGrid");
            if (GroupeRacine.Enfants.Count == 0)
                lView.Filter = null;
            else
            {
                lView.Filter = (pItem) =>
                {
                    var lOk = lFunc.Invoke((T)pItem);
                    return lOk;
                };
            }
        }
    }
}
