using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using System.Transactions;

namespace WpfComponents.Lib.Inputs.Formated
{
    public class FormatedTextBox : TextBox
    {
        #region Dependency Properties

        // Should update text when changed
        public static readonly DependencyProperty PartsProperty =
            DependencyProperty.Register(
            "Parts",
            typeof(List<object?>),
            typeof(FormatedTextBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((FormatedTextBox)o).OnPartsChanged()));

        public List<object?> Parts
        {
            get { return (List<object?>)GetValue(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public void OnPartsChanged() {
            if (Groups.Count == 0)
                ParseGroups(Format, GlobalFormat);
            FormatText(Parts);
        }
        #endregion

        #region Properties

        #region Options

        // Should call ParseGroups when changed
        public string GlobalFormat { get; set; } = "numeric|min:0|padded";
        public string Format
        {
            get;
            set;
        } = "{max:9999}/{max:12}/{max:31} alotofinbetween{max:23}:{max:59}:{max:59}";

        public bool AllowSelectionOutsideGroups { get; set; } = false;
        public bool GoToNextGroupOnMax { get; set; } = true;

        #endregion

        private int _selectedGroupIndex = -1;
        public int SelectedGroupIndex
        {
            get { return _selectedGroupIndex; }
            set
            {
                _selectedGroupIndex = value;
                if (_selectedGroupIndex < 0)
                {
                    SelectedGroup = null;
                }
                else
                {
                    SelectedGroup = Groups[value];
                }
            }
        }

        public BaseGroup? SelectedGroup { get; set; }
        public List<BaseGroup> Groups = new List<BaseGroup>();

        private string _outputFormat = "";
        private bool _isSelectionChanging = false;

        #endregion

        #region UI Events
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            bool validInput = SelectedGroup?.HandleInput(e.Text) ?? false;

            if (validInput)
            {
                UpdateParts();
                // TODO : raise event value changed
            }

            e.Handled = true;
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isSelectionChanging)
                return;

            base.OnSelectionChanged(e);

            int currentRegexGroupIndex = -1;
            for(int i = 0; i < Groups.Count; i++)
            {
                if (SelectionStart >= Groups[i].Index &&
                    SelectionStart <= Groups[i].Index + Groups[i].Length)
                {
                    currentRegexGroupIndex = i;
                    break;
                }
            }
            // Index of the group minus the first group (the global match)
            SelectedGroupIndex = currentRegexGroupIndex;

            if (SelectedGroupIndex < 0 && AllowSelectionOutsideGroups == false)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }

            _isSelectionChanging = true;
            SelectedGroup?.HandleSelection();
            _isSelectionChanging = false;
        }
        #endregion

        #region Input / Output

        private void UpdateParts()
        {
            Parts = Groups.Select(g => g.Value).ToList();
            FormatText(Parts);
        }

        private void FormatText(IEnumerable<object?> parts)
        {
            // Change text and prevent selection from changing
            _isSelectionChanging = true;
            int selectionStart = SelectionStart;
            int selectionLength = SelectionLength;
            this.Text = string.Format(_outputFormat, parts.ToArray());
            Select(selectionStart, selectionLength);
            _isSelectionChanging = false;
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // If escape unfocus the textbox
            if (e.Key == Key.Escape)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }
            // If tab select next group
            else if (e.Key == Key.Tab)
            {
                ChangeSelectedGroup(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1);
                e.Handled = true;
            }
            // If arrow keys change group
            else if (e.Key == Key.Left)
            {
                ChangeSelectedGroup(-1);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                ChangeSelectedGroup(1);
                e.Handled = true;
            }
            // If suppr
            else if (e.Key == Key.Delete)
            {
                if (SelectedGroup != null)
                {
                    SelectedGroup.HandleDelete();
                    UpdateParts();
                }
                e.Handled = true;
            }
            // Backspace
            else if (e.Key == Key.Back)
            {
                if (SelectedGroup != null)
                {
                    SelectedGroup.HandleDelete();
                    UpdateParts();
                }
                e.Handled = true;
            }

        }

        #endregion

        #region Parsing
        public void ParseGroups(string format, string globalFormat)
        {
            Groups.Clear();
            List<object> groups = ParseFormatString(format, globalFormat);

            // Create format for output
            StringBuilder outputFormatBuilder = new StringBuilder();
            int paramIndex = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i] is BaseGroup groupParam)
                {
                    Groups.Add(groupParam);
                    outputFormatBuilder.Append("{" + paramIndex + groupParam.StringFormat + "}");
                    paramIndex++;
                }
                else if (groups[i] is string outputPart)
                {
                    outputFormatBuilder.Append(outputPart);
                }
            }

            _outputFormat = outputFormatBuilder.ToString();
            // Update group values from parts
            for (int i = 0; i < Groups.Count; i++)
            {
                Groups[i].Value = Parts[i];
            }
        }

        private List<object> ParseFormatString(string format, string globalFormat)
        {
            GroupsFactory groupsFactory = new GroupsFactory();
            StringBuilder outputFormatBuilder = new StringBuilder();
            List<object> groups = new List<object>();

            StringBuilder groupBuilder = new StringBuilder();
            int depth = 0;
            int index = 0;
            char previousChar = '\0';
            foreach (char c in format)
            {
                if (c == '}' && previousChar != '\\')
                    depth -= 1;

                if (depth > 0)
                    groupBuilder.Append(c);
                else if (c != '{' && c != '}')
                    outputFormatBuilder.Append(c);

                if (c == '{' && previousChar != '\\')
                    depth += 1;

                // Group params & mask
                if (depth == 0 && groupBuilder.Length > 0)
                {
                    var group = groupsFactory.CreateParams(this, groupBuilder.ToString(), globalFormat);
                    group.Index = index;
                    groups.Add(group);
                    groupBuilder.Clear();

                    index += group.Length;
                }
                else if (depth > 0 && outputFormatBuilder.Length > 0)
                {
                    groups.Add(outputFormatBuilder.ToString());
                    index += outputFormatBuilder.Length;
                    outputFormatBuilder.Clear();
                }

                previousChar = c;
            }

            if (depth > 0)
                throw new Exception("Invalid format, was expecting '}'");

            return groups;
        }
        #endregion

        public void ChangeSelectedGroup(int delta)
        {
            int newindex = SelectedGroupIndex + delta;
            if (newindex < 0 || newindex >= Groups.Count)
                return;
            Select(Groups[newindex].Index, 0);
        }
    }
}
