using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace UltraFiltre.Lib
{
    /// <summary>
    /// Permet d'indique le nom de la propriété cible et forcer sont type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class GenerationFiltreAttribute : Attribute
    {
        public string Prop { get; set; }
        public Type Type { get; set; }
        public bool Generer { get; set; }

        public GenerationFiltreAttribute(string pPropCible = null, bool pGenerer = true, Type pType = null)
        {
            Prop = pPropCible;
            Type = pType;
            Generer = pGenerer;
        }
    }

    public class Propriete
    {
        public string Nom { get; set; }

        public string Libelle { get; set; }

        public Type Type { get; set; }

        public bool EstSimple => Utils.EstSimple(Type);
    }

    public class Filtre : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        #endregion

        [XmlIgnore]
        public FiltreGroupe GroupeParent { get; set; }

        private EnumConjonctionFiltre? _Conjonction = EnumConjonctionFiltre.And;

        public EnumConjonctionFiltre? Conjonction
        {
            get => _Conjonction;
            set
            {
                _Conjonction = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore]
        public Type TypeProp => GroupeParent?.Props.FirstOrDefault(p => p.Nom == Propriete)?.Type;
        private string _propriete;
        public string Propriete
        {
            get { return _propriete; }
            set
            {
                _propriete = value;
                OnPropertyChanged();

                if (GroupeParent == null)
                    return;

                var lProp = GroupeParent.Props.FirstOrDefault(p => p.Nom == _propriete);
                if (lProp == null)
                    return;

                if (lProp.EstSimple)
                {
                    Expression = new FiltreValeur(this, lProp.Type);
                }
                else
                    Expression = new FiltreGroupe(this, lProp.Type);
            }
        }

        private FiltreExpression _expression;

        public FiltreExpression Expression
        {
            get { return _expression; }
            set
            {
                _expression = value;
                OnPropertyChanged();
            }
        }

        public Filtre() { }

        /// <summary>
        /// Simplification pour la création de groupes
        /// </summary>
        /// <param name="pFiltres"></param>
        public Filtre(List<Filtre> pFiltres)
        {
            Expression = new FiltreGroupe()
            {
                Enfants = new ObservableCollection<Filtre>(pFiltres)
            };
        }

        #region Methodes
        public void Deplacer(int pDelta)
        {
            // Recup l'index de l'element
            int lIndex = this.GroupeParent.Enfants.IndexOf(this);
            int lNouveauIndex = lIndex + pDelta;

            if (lNouveauIndex < 0)
            {
                // Degrouper();
                return;
            }
            else if (lNouveauIndex >= this.GroupeParent.Enfants.Count)
            {
                // Degrouper(1);
                return;
            }
            /*
            if (this.Parent.Enfants[lNouveauIndex] is FiltrePropGroupe lGroupe)
            {
                Grouper(lGroupe);
            }*/
            this.GroupeParent.Enfants.Move(lIndex, lNouveauIndex);
        }

        // Supprimer un filtre d'un group parent
        public void Supprimer()
        {
            this.GroupeParent.Enfants.Remove(this);
        }

        public void AjouterRelatifA(Filtre pFiltre, int pDelta = 0)
        {
            int lIndexCible = pFiltre.GroupeParent.Enfants.IndexOf(pFiltre) + pDelta;
            if (lIndexCible > pFiltre.GroupeParent.Enfants.Count)
                lIndexCible = pFiltre.GroupeParent.Enfants.Count;

            pFiltre.GroupeParent.Ajouter(this, lIndexCible);
        }

        public void Rafraichir(FiltreGroupe filtreGroupe)
        {
            this.GroupeParent = filtreGroupe;

            Expression.FiltreParent = this;
            Expression.Rafraichir(TypeProp);
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



    [XmlInclude(typeof(FiltreGroupe))]
    [XmlInclude(typeof(FiltreValeur))]
    public abstract class FiltreExpression : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        #endregion

        [XmlIgnore]
        public Filtre FiltreParent { get; set; }

        public abstract void Rafraichir(Type typeProp);

    }

    public class FiltreGroupe : FiltreExpression
    {
        // Propriétés lié à la classe
        [XmlIgnore]
        public List<Propriete> Props { get; set; }

        public ObservableCollection<Filtre> Enfants { get; set; } = new ObservableCollection<Filtre>();

        // Constructeur vide pour la sérialisation
        public FiltreGroupe()
        {
        }

        public FiltreGroupe(Filtre pParent, Type type)
        {
            FiltreParent = pParent;
            RecuperProps(type);
        }

        // Met a jour 
        public override void Rafraichir(Type type)
        {
            RecuperProps(type);
            foreach (var lFiltre in Enfants)
            {
                lFiltre.Rafraichir(this);
            }
        }

        public void RecuperProps(Type type)
        {
            Props = new List<Propriete>();

            // Dans le cas d'une liste on récupère les props du type de la liste
            if (Utils.EstListe(type))
                type = type.GetGenericArguments().First();

            PropertyInfo[] lPropsClass = type.GetProperties();
            // Si la class a un attribut Display avec AutoGenerateFilter = false, on ne génère que les propriétés qui ont AutoGenerateFilter = true
            var lClassGenerer = type.GetCustomAttribute<GenerationFiltreAttribute>();
            bool lGenerationParDefaut = lClassGenerer?.Generer ?? true;

            foreach (var lProp in lPropsClass)
            {
                // Si la prop doit être générée
                var lGenerationAttribute = lProp.GetCustomAttribute<GenerationFiltreAttribute>();
                bool? lGenerer = lGenerationAttribute?.Generer ?? lGenerationParDefaut;

                if (lGenerer != true)
                    continue;

                // Customisation du libellé
                var lDescAttr = lProp.GetCustomAttribute<DescriptionAttribute>();
                var lDisplayAttr = lProp.GetCustomAttribute<DisplayAttribute>();
                var lLibelle = lDisplayAttr?.Name ?? lDescAttr?.Description ?? lProp.Name;

                Props.Add(
                    new Propriete
                    {
                        Nom = lGenerationAttribute?.Prop ?? lProp.Name,
                        Libelle = lLibelle,
                        Type = lGenerationAttribute?.Type ?? lProp.PropertyType
                    });
            }

            Props = Props.OrderBy(p => p.Libelle).ToList();
        }

        public void Ajouter(Filtre pFiltre, int pIndex = -1)
        {
            if (pIndex < 0)
                Enfants.Add(pFiltre);
            else
                Enfants.Insert(pIndex, pFiltre);

            pFiltre.GroupeParent = this;
        }
    }

    public class FiltreValeur : FiltreExpression
    {
        private EnumComparaisonFiltre _Operateur;

        public EnumComparaisonFiltre Operateur
        {
            get => _Operateur;
            set
            {
                _Operateur = value;
                OnPropertyChanged();
            }
        }

        private dynamic _Valeur = null;

        public dynamic Valeur
        {
            get => _Valeur;
            set
            {
                _Valeur = value;
                OnPropertyChanged();
            }
        }

        public FiltreValeur()
        {
        }

        /// <summary>
        /// Permet de set une valeur par défaut en fonction du type
        /// </summary>
        public FiltreValeur(Filtre pParent, Type pType)
        {
            FiltreParent = pParent;
            // Permet à l'interface d'afficher une combobox avec les autres valeurs possibles

            if (pType == typeof(string))
                Operateur = EnumComparaisonFiltre.Contains;

            if (pType.IsEnum)
                Valeur = pType.GetEnumValues().GetValue(0) as Enum;
        }

        /// <summary>
        /// Pour la création de filtres programatique
        /// </summary>
        /// <param name="pOperateur"></param>
        /// <param name="pValeur"></param>
        public FiltreValeur(EnumComparaisonFiltre pOperateur, dynamic pValeur)
        {
            Operateur = pOperateur;
            Valeur = pValeur;
        }

        public override void Rafraichir(Type type)
        {
            // XmlSerializer serialies enums as integers et on a besoin de savoir quelle enum est utilisé pour affiché les valeurs possibles
            if (type.IsEnum)
                Valeur = Enum.ToObject(type, Valeur);
        }
    }
}
