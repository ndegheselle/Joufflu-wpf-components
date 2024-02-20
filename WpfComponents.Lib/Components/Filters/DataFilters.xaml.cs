using System;
using System.Collections;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using WpfComponents.Lib.Components.Filters.Data;
using WpfComponents.Lib.Logic;

namespace WpfComponents.Lib.Components.Filters
{
    // XXX : D&D désactivé pour l'instant, il faudrait ajouter la notion de groupe pour que ça fasse du sens
    public class FilterDnDHandler : BaseDnDHandler
    {
        public FilterDnDHandler(DataFilters parent, Popup popup) : base(parent, popup)
        {
        }

        protected override void ApplyDrop(object sender, DragEventArgs e)
        {
            var dropData = GetDroppedData<Data.Filter>(e.Data);
            var targetData = GetDataContext<Data.Filter>((FrameworkElement)e.OriginalSource);

            var actualParent = dropData.ParentGroup;

            dropData.Delete();
            dropData.AddRelativeTo(targetData);
        }

        protected override bool CanDrop(object sender, DragEventArgs args)
        {
            object sourceData = GetDroppedData<Data.Filter>(args.Data);
            object targetData = GetDataContext<Data.Filter>((FrameworkElement)args.OriginalSource);

            if (sourceData == null || targetData == null)
                return false;

            if (sourceData == targetData)
                return false;
            return true;
        }
    }

    public partial class DataFilters : UserControl
    {
        // Définit l'affichage des options de groupement et de conjonction
        public enum EnumDisplay
        {
            Simple,
            Complet
        }

        public static readonly DependencyProperty RootGroupProperty = DependencyProperty.Register(
            "RootGroup",
            typeof(FilterGroup),
            typeof(DataFilters),
            new PropertyMetadata(null, (o, e) => ((DataFilters)o).OnRootGroupChanged()));

        public FilterGroup RootGroup
        {
            get { return (FilterGroup)GetValue(RootGroupProperty); }
            set { SetValue(RootGroupProperty, value); }
        }

        void OnRootGroupChanged()
        {
            if (RootGroup == null)
                return;

            // Appliquer le type cible
            if (TargetType != null)
                RootGroup.GetProps(TargetType);
        }

        public static readonly DependencyProperty DisplayProperty = DependencyProperty.Register(
            "Display",
            typeof(EnumDisplay),
            typeof(DataFilters),
            new PropertyMetadata(EnumDisplay.Complet));

        public EnumDisplay Display
        {
            get { return (EnumDisplay)GetValue(DisplayProperty); }
            set { SetValue(DisplayProperty, value); }
        }

        private Type _targetType;
        public Type TargetType
        {
            get
            {
                return _targetType;
            }
            set
            {
                _targetType = value;
                if (RootGroup == null)
                    RootGroup = new FilterGroup(null, value);
                else
                    RootGroup.GetProps(value);
            }
        }

        public FilterDnDHandler GestionDnD { get; }

        #region Init
        public DataFilters()
        {
            InitializeComponent();
            GestionDnD = new FilterDnDHandler(this, this.PopupDnd) { UseMinimalDistance = false };
        }
        #endregion

        public Expression<Func<TTypeCible, bool>> GetExpression<TTypeCible>()
        { return FiltersConverter.GetExpression<TTypeCible>(RootGroup.Childrens); }


        /// <summary>
        /// Permet de filtrer automatiquement un ItemsControl (DataGrid, ListBox, ComboBox ..ect). <typeparam
        /// name="T">Type de l'objet qui a été bindé dans l'ItemsControl.</typeparam>
        /// </summary>
        public void Filter<T>(ItemsControl pDataGrid) where T : class => Filter<T>(pDataGrid.ItemsSource);

        /// <summary>
        /// Permet de filtrer automatiquement l'IEnumerable bindé sur un Control. <typeparam name="T">Type de l'objet
        /// qui a été bindé dans l'ItemsControl.</typeparam>
        /// </summary>
        public void Filter<T>(IEnumerable enumerable) where T : class
        {
            var lExpression = GetExpression<T>();
            var func = lExpression.Compile();

            ICollectionView view = CollectionViewSource.GetDefaultView(enumerable) ??
                throw new NullReferenceException("Can't get the CollectionViewSource of the datagrid");
            if (RootGroup.Childrens.Count == 0)
                view.Filter = null;
            else
            {
                view.Filter = (item) =>
                {
                    return func.Invoke((T)item);
                };
            }
        }
    }
}
