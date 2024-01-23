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
            typeof(List<object>),
            typeof(FormatedTextBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((FormatedTextBox)o).OnPartsChanged()));

        public List<object> Parts
        {
            get { return (List<object>)GetValue(PartsProperty); }
            set { SetValue(PartsProperty, value); }
        }

        public void OnPartsChanged() {
            FormatText(Parts);
        }
        #endregion

        #region Properties

        #region Options
        // Should call ParseGroups when changed
        public string GlobalFormat { get; set; } = "numeric|min:0|padded";

        public string CustomFormat
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
                    SelectedParam = null;
                }
                else
                {
                    SelectedGroup = _globalMatch.Groups[value + 1];
                    SelectedParam = _groupParams[SelectedGroupIndex];
                }
            }
        }

        public IBaseGroup? SelectedGroup { get; set; }

        private string _outputFormat = "";
        private Regex _globalRegex;
        private bool _isSelectionChanging = false;

        private List<BaseGroupParams> _groupParams = new List<BaseGroupParams>();
        #endregion
        public FormatedTextBox() { 
            ParseGroups(CustomFormat, GlobalFormat);
            GetPartsValues();
        }

        #region UI Events
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            // Get the new text that would be created (including selection handling)

            // Can't edit outside groups
            if (SelectedGroupIndex < 0)
            {
                e.Handled = true;
                return;
            }

            if (SelectedParam is NumericParams numericParam)
            {
                string newString = Parts[SelectedGroupIndex].ToString() + e.Text;

                // If the number is too big we loop back to only the new number
                if (numericParam.Length != null && newString.Length > numericParam.Length)
                    newString = e.Text;

                int newValue = int.Parse(newString);
                if (numericParam.Max != null && newValue > numericParam.Max)
                {
                    newValue = numericParam.Max.Value;
                    if (GoToNextGroupOnMax && _globalMatch.Groups.Count > SelectedGroupIndex + 2)
                        Select(_globalMatch.Groups[SelectedGroupIndex + 2].Index, 0);
                }
                else if (numericParam.Min != null && newValue > numericParam.Min)
                    newValue = numericParam.Min.Value;

                Parts[SelectedGroupIndex] = newValue;
                e.Handled = true;
            }
            else
            {
                // TODO : handle string
            }
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isSelectionChanging)
                return;

            base.OnSelectionChanged(e);

            if (_globalMatch == null)
                return;

            int currentRegexGroupIndex = -1;
            for (int i = 1; i < _globalMatch.Groups.Count; i++)
            {
                if (SelectionStart >= _globalMatch.Groups[i].Index &&
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
        private void FormatText(IEnumerable<object> parts)
        { this.Text = string.Format(_outputFormat, parts.ToArray()); }

        // XXX : not needed in theory since we diretly get the parts from PreviewTextInput
        private IEnumerable<object> GetPartsValues()
        {
            _globalMatch = _globalRegex.Match(this.Text);

            List<object> values = new List<object>();
            if (_globalMatch.Success)
            {
                for (int i = 1; i < _globalMatch.Groups.Count; i++)
                {
                    if (_groupParams[i - 1] is NumericParams)
                        values.Add(int.Parse(_globalMatch.Groups[i].Value));
                    else
                        values.Add(_globalMatch.Groups[i].Value);
                }
            }
            return values;
        }
        #endregion

        #region Parsing
        public void ParseGroups(string format, string globalFormat)
        {
            List<object> groups = ParseFormatString(format, globalFormat);

            // Create format for output
            // Create regex for input
            StringBuilder globalRegexBuilder = new StringBuilder();
            StringBuilder outputFormatBuilder = new StringBuilder();
            int paramIndex = 0;
            for (int i = 0; i < groups.Count; i++)
            {
                if (groups[i] is BaseGroupParams groupParam)
                {
                    _groupParams.Add(groupParam);
                    outputFormatBuilder.Append("{" + paramIndex + groupParam.StringFormat + "}");
                    globalRegexBuilder.Append(groupParam.Regex);
                    paramIndex++;
                }
                else if (groups[i] is string outputPart)
                {
                    outputFormatBuilder.Append(outputPart);
                    globalRegexBuilder.Append(outputPart);
                }
            }

            _outputFormat = outputFormatBuilder.ToString();
            _globalRegex = new Regex(globalRegexBuilder.ToString());
        }

        private List<object> ParseFormatString(string format, string globalFormat)
        {
            GroupParamsFactory paramsFactory = new GroupParamsFactory();
            StringBuilder outputFormatBuilder = new StringBuilder();
            List<object> groups = new List<object>();

            StringBuilder groupBuilder = new StringBuilder();
            int depth = 0;
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
                    var param = paramsFactory.CreateParams(groupBuilder.ToString(), globalFormat);
                    groups.Add(param);
                    groupBuilder.Clear();
                }
                else if (depth > 0 && outputFormatBuilder.Length > 0)
                {
                    groups.Add(outputFormatBuilder.ToString());
                    outputFormatBuilder.Clear();
                }

                previousChar = c;
            }

            if (depth > 0)
                throw new Exception("Invalid format, was expecting '}'");

            return groups;
        }
        #endregion
    }
}
