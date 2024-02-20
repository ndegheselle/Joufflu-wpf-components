using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace WpfComponents.Lib.Components.Filters.Data
{
    /// <summary>
    /// Permet d'indique le nom de la propriété cible et forcer sont type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class FilterGenerationAttribute : Attribute
    {
        public string Prop { get; set; }
        public Type Type { get; set; }
        public bool Generate { get; set; }

        public FilterGenerationAttribute(string pPropCible = null, bool pGenerer = true, Type pType = null)
        {
            Prop = pPropCible;
            Type = pType;
            Generate = pGenerer;
        }
    }

    public class Property
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public Type Type { get; set; }

        public bool IsSimple => Utils.IsSimple(Type);
    }

    public class Filter : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        #endregion

        [XmlIgnore]
        public FilterGroup ParentGroup { get; set; }

        private EnumConjunctionFilter? _Conjunction = EnumConjunctionFilter.And;

        public EnumConjunctionFilter? Conjunction
        {
            get => _Conjunction;
            set
            {
                _Conjunction = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public Type TypeProp => ParentGroup?.Props.FirstOrDefault(p => p.Name == Property)?.Type;
        private string _proprerty;
        public string Property
        {
            get { return _proprerty; }
            set
            {
                _proprerty = value;
                OnPropertyChanged();

                if (ParentGroup == null)
                    return;

                var lProp = ParentGroup.Props.FirstOrDefault(p => p.Name == _proprerty);
                if (lProp != null)
                {

                    if (lProp.IsSimple)
                    {
                        Expression = new FilterValue(this, lProp.Type);
                    }
                    else
                    {
                        Expression = new FilterGroup(this, lProp.Type);
                    }
                }
                else
                {
                    Expression = null;
                }
            }
        }

        private FilterExpression _expression;

        public FilterExpression Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                OnPropertyChanged();
            }
        }

        public Filter() { }

        /// <summary>
        /// Simplification pour la création de groupes
        /// </summary>
        /// <param name="pFiltres"></param>
        public Filter(List<Filter> pFiltres)
        {
            Expression = new FilterGroup()
            {
                Childrens = new ObservableCollection<Filter>(pFiltres)
            };
        }

        #region Methodes
        public void Move(int delta)
        {
            // Recup l'index de l'element
            int index = this.ParentGroup.Childrens.IndexOf(this);
            int newIndex = index + delta;

            if (newIndex < 0)
            {
                // Degrouper();
                return;
            }
            else if (newIndex >= this.ParentGroup.Childrens.Count)
            {
                // Degrouper(1);
                return;
            }
            /*
            if (this.Parent.Enfants[lNouveauIndex] is FiltrePropGroupe lGroupe)
            {
                Grouper(lGroupe);
            }*/
            this.ParentGroup.Childrens.Move(index, newIndex);
        }

        // Supprimer un filtre d'un group parent
        public void Delete()
        {
            this.ParentGroup.Childrens.Remove(this);
        }

        public void AddRelativeTo(Filter pFiltre, int pDelta = 0)
        {
            int lIndexCible = pFiltre.ParentGroup.Childrens.IndexOf(pFiltre) + pDelta;
            if (lIndexCible > pFiltre.ParentGroup.Childrens.Count)
                lIndexCible = pFiltre.ParentGroup.Childrens.Count;

            pFiltre.ParentGroup.Add(this, lIndexCible);
        }

        public void Refresh(FilterGroup filtreGroupe)
        {
            this.ParentGroup = filtreGroupe;

            Expression.ParentFilter = this;
            Expression.Refresh(TypeProp);
        }

        /* Pour le jour ou on aura besoin de grouper/dégrouper
        public void Grouper(FiltrePropGroupe lGroupe = null)
        {
            int lIndex = -1;
            if (lGroupe == null)
            {
                var lVoisins = this.Parent.Enfants;
                // Récupérer le voisin avant et après si il existe
                var lVoisinAvant = lVoisins.ElementAtOrDefault(lVoisins.IndexOf(this) - 1);
                var lVoisinApres = lVoisins.ElementAtOrDefault(lVoisins.IndexOf(this) + 1);

                // Recherche d'un groupe voisin a grouper
                if (lVoisinAvant is FiltrePropGroupe lGroupeAvant)
                {
                    lGroupe = lGroupeAvant;
                }
                else if (lVoisinApres is FiltrePropGroupe lGroupeApres)
                {
                    lGroupe = lGroupeApres;
                    // Ajout au début de la liste
                    lIndex = 0;
                }
                else
                {
                    // Pas de groupe voisin, on en crée un nouveau
                    lGroupe = new FiltrePropGroupe();
                    this.AjouterRelatifA(lGroupe);
                }
            }

            Supprimer();
            lGroupe.Ajouter(this, lIndex);
        }

        public void Degrouper(int pDelta = 0)
        {
            if (this.Parent.Parent == null)
                return;

            var lParentActuel = this.Parent;
            Supprimer();
            AjouterRelatifA(lParentActuel, pDelta);

            lParentActuel.NettoyerGroupe();
        }
        */
        #endregion
    }



    [XmlInclude(typeof(FilterGroup))]
    [XmlInclude(typeof(FilterValue))]
    public abstract class FilterExpression : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        #endregion

        [XmlIgnore]
        public Filter ParentFilter { get; set; }

        public abstract void Refresh(Type typeProp);

    }

    public class FilterGroup : FilterExpression
    {
        // Propriétés lié à la classe
        [XmlIgnore]
        public List<Property> Props { get; set; }

        public ObservableCollection<Filter> Childrens { get; set; } = new ObservableCollection<Filter>();

        // Constructeur vide pour la sérialisation
        public FilterGroup()
        {
        }

        public FilterGroup(Filter pParent, Type type)
        {
            ParentFilter = pParent;
            GetProps(type);
        }

        // Met a jour 
        public override void Refresh(Type type)
        {
            GetProps(type);
            foreach (var lFiltre in Childrens)
            {
                lFiltre.Refresh(this);
            }
        }

        public void GetProps(Type type)
        {
            Props = new List<Property>();

            // Dans le cas d'une liste on récupère les props du type de la liste
            if (Utils.IsList(type))
                type = type.GetGenericArguments().First();

            PropertyInfo[] propClass = type.GetProperties();
            // Si la class a un attribut Display avec AutoGenerateFilter = false, on ne génère que les propriétés qui ont AutoGenerateFilter = true
            var generateClass = type.GetCustomAttribute<FilterGenerationAttribute>();
            bool defaultGeneration = generateClass?.Generate ?? true;

            foreach (var prop in propClass)
            {
                // Si la prop doit être générée
                var generationAttr = prop.GetCustomAttribute<FilterGenerationAttribute>();
                bool? generate = generationAttr?.Generate ?? defaultGeneration;

                if (generate != true)
                    continue;

                Type propType = prop.PropertyType;
                if (Nullable.GetUnderlyingType(prop.PropertyType) is Type nullablePropType)
                    propType = nullablePropType;

                // Customisation du libellé
                var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
                var displayAttr = prop.GetCustomAttribute<DisplayAttribute>();
                var description = displayAttr?.Name ?? descAttr?.Description ?? prop.Name;

                Props.Add(
                    new Property
                    {
                        Name = generationAttr?.Prop ?? prop.Name,
                        Description = description,
                        Type = generationAttr?.Type ?? propType
                    });
            }

            Props = Props.OrderBy(p => p.Description).ToList();
        }

        public void Add(Filter pFiltre, int pIndex = -1)
        {
            if (pIndex < 0)
                Childrens.Add(pFiltre);
            else
                Childrens.Insert(pIndex, pFiltre);

            pFiltre.ParentGroup = this;
        }
    }

    public class FilterValue : FilterExpression
    {
        private EnumOperatorFilter _operator;

        public EnumOperatorFilter Operator
        {
            get => _operator;
            set
            {
                _operator = value;
                OnPropertyChanged();
            }
        }

        private dynamic _value = null;

        public dynamic Value
        {
            get => _value;
            set
            {
                _value = value;
                OnPropertyChanged();
            }
        }

        public FilterValue()
        {
        }

        /// <summary>
        /// Permet de set une valeur par défaut en fonction du type
        /// </summary>
        public FilterValue(Filter parent, Type type)
        {
            ParentFilter = parent;
            // Permet à l'interface d'afficher une combobox avec les autres valeurs possibles

            if (type == typeof(string))
                Operator = EnumOperatorFilter.Contains;

            if (type.IsEnum)
                Value = type.GetEnumValues().GetValue(0) as Enum;
        }

        /// <summary>
        /// Pour la création de filtres programatique
        /// </summary>
        /// <param name="pOperateur"></param>
        /// <param name="pValeur"></param>
        public FilterValue(EnumOperatorFilter pOperateur, dynamic pValeur)
        {
            Operator = pOperateur;
            Value = pValeur;
        }

        public override void Refresh(Type type)
        {
            // XmlSerializer serialies enums as integers et on a besoin de savoir quelle enum est utilisé pour affiché les valeurs possibles
            if (type.IsEnum)
                Value = Enum.ToObject(type, Value);
        }
    }
}
