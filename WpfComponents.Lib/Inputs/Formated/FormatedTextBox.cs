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

namespace WpfComponents.Lib.Inputs.Formated
{
    public class FormatedTextBox : TextBox
    {
        #region Dependency Properties

        // Should update text when changed
        public static readonly DependencyProperty PartsProperty =
            DependencyProperty.Register(
            "Parts",
            typeof(List<dynamic>),
            typeof(FormatedTextBox),
            new PropertyMetadata(null, (o, e) => ((FormatedTextBox)o).OnPartsChanged()));

        public List<dynamic> Parts
        {
            get { return (List<dynamic>)GetValue(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public void OnPartsChanged()
        {
            FormatText(Parts);
            GetGroupsValues();
        }
        #endregion

        #region Properties

        #region Options
        // Should call ParseGroups when changed
        public string GlobalFormat { get; set; } = "numeric|min:0|padded";
        public string CustomFormat { get; set; } = "{max:9999}/{max:12}/{max:31} alotofinbetween{max:23}:{max:59}:{max:59}";
        public bool AllowSelectionOutsideGroups { get; set; } = false;
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
                    SelectedParam = null;
                }
                else
                {
                    SelectedGroup = _globalMatch.Groups[value + 1];
                    SelectedParam = _groupParams[SelectedGroupIndex];
                }
            }
        }
        public BaseGroupParams? SelectedParam { get; set; }
        public Group? SelectedGroup { get; set; }

        private string _outputFormat = "";
        private Regex _globalRegex;
        private Match _globalMatch;
        private bool _isSelectionChanging = false;

        private List<BaseGroupParams> _groupParams = new List<BaseGroupParams>();
        #endregion
        
        public FormatedTextBox() { ParseGroups(CustomFormat, GlobalFormat); }

        #region UI Events
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            // Get the new text that would be created (including selection handling)

            e.Handled = true;
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isSelectionChanging)
                return;

            base.OnSelectionChanged(e);

            if(_globalMatch == null)
                return;

            int currentRegexGroupIndex = -1;
            for (int i = 1; i < _globalMatch.Groups.Count; i++)
            {
                if(SelectionStart >= _globalMatch.Groups[i].Index &&
                    SelectionStart <= _globalMatch.Groups[i].Index + _globalMatch.Groups[i].Length)
                {
                    currentRegexGroupIndex = i;
                    break;
                }
            }
            // Index of the group minus the first group (the global match)
            SelectedGroupIndex = currentRegexGroupIndex - 1;

            if (SelectedGroupIndex < 0 && AllowSelectionOutsideGroups == false)
            {
                Keyboard.ClearFocus();
                e.Handled = true;
            }

            if (SelectedGroupIndex < 0)
                return;

            // For numeric, select the whole number
            if (SelectedParam is NumericParams)
            {
                _isSelectionChanging = true;
                this.Select(SelectedGroup.Index, SelectedGroup.Length);
                _isSelectionChanging = false;
            }
        }
        #endregion

        #region Input / Output
        private void FormatText(List<dynamic> parts) { this.Text = string.Format(_outputFormat, parts.ToArray()); }

        private List<string> GetGroupsValues()
        {
            _globalMatch = _globalRegex.Match(this.Text);

            if(!_globalMatch.Success)
                return new List<string>();
            else
                return _globalMatch.Groups.Cast<Group>().Skip(1).Select(x => x.Value).ToList();
        }
        #endregion

        #region Parsing
        public void ParseGroups(string format, string globalFormat)
        {
            List<dynamic> groups = ParseFormatString(format, globalFormat);

            // Create format for output
            // Create regex for input
            StringBuilder globalRegexBuilder = new StringBuilder();
            StringBuilder outputFormatBuilder = new StringBuilder();
            int paramIndex = 0;
            for(int i = 0; i < groups.Count; i++)
            {
                if(groups[i] is BaseGroupParams groupParam)
                {
                    _groupParams.Add(groupParam);
                    outputFormatBuilder.Append("{" + paramIndex + groupParam.StringFormat + "}");
                    globalRegexBuilder.Append(groupParam.Regex);
                    paramIndex++;
                } else if(groups[i] is string)
                {
                    outputFormatBuilder.Append(groups[i]);
                    globalRegexBuilder.Append(Regex.Escape(groups[i]));
                }
            }

            _outputFormat = outputFormatBuilder.ToString();
            _globalRegex = new Regex(globalRegexBuilder.ToString());
        }

        private List<dynamic> ParseFormatString(string format, string globalFormat)
        {
            GroupParamsFactory paramsFactory = new GroupParamsFactory();
            StringBuilder outputFormatBuilder = new StringBuilder();
            List<dynamic> groups = new List<dynamic>();

            StringBuilder groupBuilder = new StringBuilder();
            int depth = 0;
            char previousChar = '\0';
            foreach(char c in format)
            {
                if(c == '}' && previousChar != '\\')
                    depth -= 1;

                if(depth > 0)
                    groupBuilder.Append(c);
                else if(c != '{' && c != '}')
                    outputFormatBuilder.Append(c);

                if(c == '{' && previousChar != '\\')
                    depth += 1;

                // Group params & mask
                if(depth == 0 && groupBuilder.Length > 0)
                {
                    var param = paramsFactory.CreateParams(groupBuilder.ToString(), globalFormat);
                    groups.Add(param);
                    groupBuilder.Clear();
                } else if(depth > 0 && outputFormatBuilder.Length > 0)
                {
                    groups.Add(outputFormatBuilder.ToString());
                    outputFormatBuilder.Clear();
                }

                previousChar = c;
            }

            if(depth > 0)
                throw new Exception("Invalid format, was expecting '}'");

            return groups;
        }
        #endregion
    }
}
