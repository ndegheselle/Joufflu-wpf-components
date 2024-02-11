using GerCRUD_FW3ma_Namespace.Frameworks3ma.nsWpfOutils.Interne.Controles.ExplorateurFichiers.DnD;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Converters;
using System.Windows.Threading;

namespace WpfComponents.Lib.Components.FileExplorer.Controls
{
    /// <summary>
    /// Logique partagé entre ExplorateurFichiersListe et ExplorateurFichiersTree
    /// </summary>
    public abstract class FileExplorerBase : UserControl, INotifyPropertyChanged
    {
        [Flags]
        public enum EnumAutorisation 
        {
            Rien = 0,
            AutoriseDrop = 1,
            AutoriseDrag = 2,
            AutoriseDragDrop = AutoriseDrag | AutoriseDrop,
            AutoriseRaccourcis = 4,
            AutoriseContextMenuOuvrir = 8,
            AutoriseContextMenuActions = 16,
            AutoriseContextMenu = AutoriseContextMenuOuvrir | AutoriseContextMenuActions,
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        private string _TexteStatut = "";

        public string TexteStatut
        {
            get { return _TexteStatut; }
            set
            {
                _TexteStatut = value;
                OnPropertyChanged();
            }
        }

        public GestionDnD GestionDnD { get; private set; }

        public TextBox TextBoxEnCoursEdition { get; set; } = null;

        private bool _EstEnCoursDeNavigation = false;
        private int _IndexeHistoriqueActuel = -1;
        private List<NodeExplorateurDossier> _HistoriqueNavigation = new List<NodeExplorateurDossier>();
        private readonly DispatcherTimer _TimerRenommage;

        // Permet d'éviter qu'un click sur une node trigger le renommage imédiatement
        protected bool _EmpecherRenommage { get; set; } = false;

        private ScrollViewer _ScrollParent;

        public abstract IEnumerable<NodeExplorateur> NodeSelectionnees { get; }

        public abstract PopupActionDnD PopupDnD { get; }

        public IEnumerable<string> CheminsSelectionnees { get { return NodeSelectionnees.Select(x => x.Chemin); } }

        public FileExplorerBase()
        {
            GestionDnD = new GestionDnD(this);
            _TimerRenommage = new DispatcherTimer(
                new TimeSpan(0, 0, 0, 0, GetDoubleClickTime()),
                DispatcherPriority.Normal,
                GererClickRenommage,
                Dispatcher.CurrentDispatcher)
            {
                IsEnabled = false
            };
        }

        #region Dependency Properties
        public static readonly DependencyProperty NodeRacineProperty = DependencyProperty.Register(
            "NodeRacine",
            typeof(NodeExplorateurDossier),
            typeof(FileExplorerBase),
            new UIPropertyMetadata(null, (o, value) => ((FileExplorerBase)o).OnNodeRacineChange(value)));

        public NodeExplorateurDossier NodeRacine
        {
            get { return (NodeExplorateurDossier)GetValue(NodeRacineProperty); }
            set { SetValue(NodeRacineProperty, value); }
        }

        public static readonly DependencyProperty AutorisationsProperty = DependencyProperty.Register(
            "Autorisations",
            typeof(EnumAutorisation),
            typeof(FileExplorerBase),
            new UIPropertyMetadata(
                EnumAutorisation.AutoriseDragDrop |
                    EnumAutorisation.AutoriseRaccourcis |
                    EnumAutorisation.AutoriseContextMenu));

        public EnumAutorisation Autorisations
        {
            get { return (EnumAutorisation)GetValue(AutorisationsProperty); }
            set { SetValue(AutorisationsProperty, value); }
        }

        public static readonly DependencyProperty MediateurProperty = DependencyProperty.Register(
            "Mediateur",
            typeof(IMediateurSelectionFichier),
            typeof(FileExplorerBase),
            new UIPropertyMetadata(null, (o, value) => ((FileExplorerBase)o).OnMediateurChange()));

        private void OnMediateurChange()
        {
            if (Mediateur == null)
                return;
            // Si un autre controlleur sélectionne un élément on clear notre sélection
            Mediateur.OnSelectionNode += (sender, value) =>
            {
                if (sender != this)
                {
                    ClearSelection();
                }
            };
        }

        public IMediateurSelectionFichier Mediateur
        {
            get { return (IMediateurSelectionFichier)GetValue(MediateurProperty); }
            set { SetValue(MediateurProperty, value); }
        }
        #endregion

        #region Methodes
        public abstract void ClearSelection();

        public void CreerNouveauDossier(NodeExplorateurDossier pNodeCibleDossier)
        {
            string lNom = SystemFichier.NomNouveauDossierValide(pNodeCibleDossier.Chemin);

            Directory.CreateDirectory(Path.Combine(pNodeCibleDossier.Chemin, lNom));
            var lNouveauDossier = new NodeExplorateurDossier(Path.Combine(pNodeCibleDossier.Chemin, lNom));
            pNodeCibleDossier.Ajouter(lNouveauDossier);
            lNouveauDossier.EstSelectionnee = true;
            RenommerNode(lNouveauDossier);
        }
        #endregion

        #region Events
        protected virtual void OnNodeRacineChange(DependencyPropertyChangedEventArgs eventArgs)
        {
            if (!_EstEnCoursDeNavigation)
                AjouterHistorique(NodeRacine);
        }

        protected void HandleNodeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TextBoxEnCoursEdition != null)
                return;

            NodeExplorateur lDataContext = (e.OriginalSource as FrameworkElement).DataContext as NodeExplorateur;
            if (lDataContext == null)
                return;

            _EmpecherRenommage = true;
            NaviguerNodes(e, new List<NodeExplorateur>() { lDataContext });
            _TimerRenommage.Stop();
            e.Handled = true;
        }

        // Si le click est effectuer sur un élément déjà sélectionné et qu'on a pas fait un double click on renomme
        protected void HandleNodeMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_EmpecherRenommage)
            {
                _EmpecherRenommage = false;
                return;
            }
            if (e.ChangedButton != MouseButton.Left)
                return;
            _TimerRenommage.Start();
        }

        protected virtual void HandlePreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Avant AutoriseRaccourcis étant donné que c'est pas uen option qui devrais être bloquée
            GererRaccourcisRenommage(sender, e);

            if (Autorisations.HasFlag(EnumAutorisation.AutoriseRaccourcis) == false ||
                TextBoxEnCoursEdition != null ||
                e.Handled)
                return;

            // Go back (si aucun go back la racine)
            if (e.Key == Key.Back)
            {
                NaviguerParent();
                e.Handled = true;
            }
            // Coller
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.V)
            {
                NodeExplorateurDossier lDestination = NodeSelectionnees.FirstOrDefault() as NodeExplorateurDossier;
                if (lDestination == null)
                    lDestination = NodeRacine;

                if (ExplorateurFichierCmds.CollerPressePapier.CanExecute(lDestination.Chemin))
                {
                    ExplorateurFichierCmds.CollerPressePapier.Execute(lDestination.Chemin);
                    e.Handled = true;
                }
            }
            else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) &&
                e.Key == Key.N)
            {
                CreerNouveauDossier(NodeRacine);
                e.Handled = true;
            }

            if (!NodeSelectionnees.Any())
                return;

            // Couper
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.X)
            {
                IEnumerable<string> lFichierSelectionnes = CheminsSelectionnees;
                ExplorateurFichierCmds.CouperPressePapier.Execute(lFichierSelectionnes);
                e.Handled = true;
            }
            // Copier
            else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.C)
            {
                IEnumerable<string> lFichierSelectionnes = CheminsSelectionnees;
                ExplorateurFichierCmds.CopierPressePapier.Execute(lFichierSelectionnes);
                e.Handled = true;
            }
            // Ouvrir
            else if (e.Key == Key.Enter)
            {
                NaviguerNodes(e, NodeSelectionnees);
            }
            // Supprimer
            else if (e.Key == Key.Delete)
            {
                if (ExplorateurFichierCmds.Supprimer.CanExecute(CheminsSelectionnees))
                {
                    ExplorateurFichierCmds.Supprimer.Execute(CheminsSelectionnees);
                    e.Handled = true;
                }
            }
            // Renomer
            else if (e.Key == Key.F2)
            {
                RenommerNode(NodeSelectionnees.First());
                e.Handled = true;
            }
        }

        // 2 cas de figures :
        // - On a plusieurs TreeView l'une après l'autre, on veut un scrollviewer parent pour les gérer tous
        // - On a un TreeView et on veut le laisser gérer sont propre scroll sans a avoir a ajouter un scrollviewer
        protected void PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_ScrollParent == null)
                _ScrollParent = VisualTreeHelperExt.GetParent<ScrollViewer>(this);

            if (e.Handled == true || _ScrollParent == null)
                return;

            e.Handled = true;
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
            eventArg.RoutedEvent = UIElement.MouseWheelEvent;
            eventArg.Source = sender;
            _ScrollParent.RaiseEvent(eventArg);
        }
        #endregion

        #region Renommage
        public void RenommerNode(NodeExplorateur pNode)
        {
            TextBoxEnCoursEdition = RecupererTextBoxEdition(pNode);
            if (TextBoxEnCoursEdition == null)
                return;

            if (pNode is NodeExplorateurDossier)
            {
                TextBoxEnCoursEdition.SelectAll();
            }
            else
            {
                // Selectionner seulement le nom du fichier et pas l'extension
                int lTailleNomFichier = pNode.Nom.LastIndexOf(".");
                if (lTailleNomFichier > 0)
                    TextBoxEnCoursEdition.Select(0, lTailleNomFichier);
                else
                    TextBoxEnCoursEdition.SelectAll();
            }

            pNode.EstEnEdition = true;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        protected static extern int GetDoubleClickTime();

        protected void ClearRenommage()
        {
            if (TextBoxEnCoursEdition == null)
                return;
            var lNode = TextBoxEnCoursEdition.DataContext as NodeExplorateur;
            lNode.EstEnEdition = false;
            TextBoxEnCoursEdition.Select(0, 0);
            TextBoxEnCoursEdition = null;
            // Permet de fix des comportement bizarre de la selection après un renommage (élément sélectionné pour toujours ?)
            ClearSelection();
        }

        protected void AnnulerRenommage()
        {
            if (TextBoxEnCoursEdition == null)
                return;
            var lNode = TextBoxEnCoursEdition.DataContext as NodeExplorateur;
            TextBoxEnCoursEdition.Text = lNode.Nom;
            ClearRenommage();
        }

        protected abstract TextBox RecupererTextBoxEdition(NodeExplorateur pNode);

        protected void TextBoxNode_LostFocus(object sender, RoutedEventArgs e)
        {
            var lTextBox = (TextBox)sender;
            var lNode = lTextBox.DataContext as NodeExplorateur;

            if (lNode.EstEnEdition == false)
                return;

            lNode.EstEnEdition = false;

            // Si changement du nom
            if (lTextBox.Text != lNode.Nom)
            {
                var lParams = new FichierCmdParams(
                    lNode.Chemin,
                    Path.Combine(Path.GetDirectoryName(lNode.Chemin), lTextBox.Text));
                if (ExplorateurFichierCmds.Renommer.CanExecute(lParams) == false)
                {
                    AnnulerRenommage();
                    MessageBox.Show(
                        "Un fichier du même nom existe déjà à cet emplacement.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    ExplorateurFichierCmds.Renommer.Execute(lParams);
                }
                catch (OperationCanceledException)
                {
                    AnnulerRenommage();
                    return;
                }
            }

            ClearRenommage();
            e.Handled = true;
        }

        protected void NomNode_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // N'accepter que les charactères autorisés
            Regex lRegex = new Regex(@"^[\w\-. ]+$");
            if (!lRegex.IsMatch(e.Text))
                e.Handled = true;
            base.OnPreviewTextInput(e);
        }


        private void GererRaccourcisRenommage(object sender, KeyEventArgs e)
        {
            if (TextBoxEnCoursEdition == null)
                return;

            if (e.Key == Key.Escape)
            {
                AnnulerRenommage();
                e.Handled = true;
            }
            else if (e.Key == Key.Return)
            {
                // On force le focus sur le control pour que le LostFocus soit appelé sur le TextBox
                this.Focus();
                e.Handled = true;
            }
        }

        private void GererClickRenommage(object sender, EventArgs e)
        {
            // Garde fou
            if (NodeSelectionnees.Any() == false)
                return;

            _TimerRenommage.Stop();
            RenommerNode(NodeSelectionnees.First());
        }
        #endregion

        #region Navigation
        protected void NaviguerNodes(RoutedEventArgs e, IEnumerable<NodeExplorateur> pNodes)
        {
            if (pNodes.Count() == 0)
                return;

            var lPremierNode = pNodes.First();

            if (lPremierNode is NodeExplorateurDossier lNodeDossier)
            {
                NaviguerDossier(lNodeDossier);
                e.Handled = true;
            }
            // Si fichier on ouvre tout les fichiers
            else if (lPremierNode is NodeExplorateurFichier)
            {
                ExplorateurFichierCmds.Ouvrir.Execute(pNodes.Where(x => x is NodeExplorateurFichier).Select(x => x.Chemin));
                e.Handled = true;
            }
        }

        protected virtual void NaviguerDossier(NodeExplorateurDossier pNodeDossier)
        {
            // A overide dans la Liste pour set NodeRacine = pNodeDossier
            pNodeDossier.EstOuvert = true;
        }

        public void NaviguerParent()
        {
            NodeExplorateur lNodeParent = NodeSelectionnees.FirstOrDefault()?.Parent ?? NodeRacine;

            // On remonte le parent du parent
            if (lNodeParent.Parent == null)
                return;

            NaviguerDossier(lNodeParent.Parent);
        }
        #endregion

        #region Historique navigation
        protected void AjouterHistorique(NodeExplorateurDossier pNode)
        {
            if (_IndexeHistoriqueActuel < _HistoriqueNavigation.Count - 1)
            {
                _HistoriqueNavigation.RemoveRange(
                    _IndexeHistoriqueActuel + 1,
                    _HistoriqueNavigation.Count - _IndexeHistoriqueActuel - 1);
            }
            _HistoriqueNavigation.Add(pNode);
            _IndexeHistoriqueActuel = _HistoriqueNavigation.Count - 1;
        }

        public void NaviguerEnArriere()
        {
            if (_IndexeHistoriqueActuel - 1 < 0)
                return;

            _IndexeHistoriqueActuel -= 1;
            _EstEnCoursDeNavigation = true;
            NodeRacine = _HistoriqueNavigation[_IndexeHistoriqueActuel];
            _EstEnCoursDeNavigation = false;
        }

        public void NaviguerEnAvant()
        {
            // Si rien dans l'historique ou qu'on est déjà à la fin
            if (_IndexeHistoriqueActuel + 1 >= _HistoriqueNavigation.Count)
                return;

            _IndexeHistoriqueActuel += 1;

            _EstEnCoursDeNavigation = true;
            NodeRacine = _HistoriqueNavigation[_IndexeHistoriqueActuel];
            _EstEnCoursDeNavigation = false;
        }
        #endregion

        #region DragDrop events

        // XXX : Peut être il y a un moyen d'appeler un event Handler dans une prop (GestionDnD.HandleDragMouseDown directement dans le XAML) mais je crois pas

        protected void HandleMouseDown(object sender, MouseButtonEventArgs e)
        { GestionDnD.HandleDragMouseDown(sender, e); }

        protected void HandleMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        { GestionDnD.HandleDragMouseMove(sender, e); }

        protected void HandleDragOver(object sender, DragEventArgs e) { GestionDnD.HandleDragOver(sender, e); }

        protected void HandleDragLeave(object sender, DragEventArgs e) { GestionDnD.HandleDragLeave(sender, e); }

        protected void HandleDrop(object sender, DragEventArgs e) { GestionDnD.HandleDrop(sender, e); }
        #endregion

        #region Contexte menu
        protected virtual void ContextMenu_OnOpening(object sender, ContextMenuEventArgs e)
        {
            var lSource = e.Source as FrameworkElement;
            var lSourceOrigine = e.OriginalSource as FrameworkElement;
            NodeExplorateur pNodeCible = NodeSelectionnees.FirstOrDefault() ?? NodeRacine;

            if ((Autorisations & EnumAutorisation.AutoriseContextMenu) == 0)
            {
                lSource.ContextMenu.IsOpen = false;
                e.Handled = true;
                return;
            }

            // Permet de gérer un clique droit hors de la TreeView (étant donnés que les éléments restent sélectionnés)
            if (lSourceOrigine?.DataContext == null)
                pNodeCible = NodeRacine;

            lSource.ContextMenu = new ContextMenuExplorateur(this, pNodeCible);
        }
        #endregion
    }
}
