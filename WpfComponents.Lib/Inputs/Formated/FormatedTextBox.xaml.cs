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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace WpfComponents.Lib.Inputs.Formated
{
    public class FormatedTextBox : TextBox, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public event EventHandler<List<object?>>? ValuesChanged;
        #region Dependency Properties

        // Should update text when changed
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(
            "Values",
            typeof(List<object?>),
            typeof(FormatedTextBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((FormatedTextBox)o).OnValuesChanged()));

        public List<object?> Values
        {
            get { return (List<object?>)GetValue(ValuesProperty); }
            set { SetValue(ValuesProperty, value); }
        }

        private void OnValuesChanged()
        {
            if (Groups.Count == 0)
                ParseGroups(Format, GlobalFormat);
            ValuesChanged?.Invoke(this, Values);
            FormatText(Groups);
        }
        #endregion

        #region Properties

        #region Options
        private string _globalFormat = "";

        public string GlobalFormat
        {
            get { return _globalFormat; }
            set
            {
                _globalFormat = value;
                ParseGroups(Format, GlobalFormat);
                FormatText(Groups);
            }
        }

        private string _format = "";

        public string Format
        {
            get { return _format; }
            set
            {
                _format = value;
                ParseGroups(Format, GlobalFormat);
                FormatText(Groups);
            }
        }

        public bool AllowSelectionOutsideGroups { get; set; } = false;

        public bool ShowDeleteButton { get; set; } = true;

        public bool ShowIncrementsButtons { get; set; } = true;
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

        // UI Parts
        private Button _clearButton;
        private Button _upButton;
        private Button _downButton;
        #endregion

        #region Init
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _clearButton = (Button)this.Template.FindName("PART_ClearButton", this);
            _upButton = (Button)this.Template.FindName("PART_UpButton", this);
            _downButton = (Button)this.Template.FindName("PART_DownButton", this);

            _clearButton.Click += ClearButton_Click;
            _upButton.Click += UpButton_Click;
            _downButton.Click += DownButton_Click;
        }
        #endregion

        #region UI Events
        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            bool validInput = SelectedGroup?.OnInput(e.Text) ?? false;
            if (validInput)
            {
                UpdateCurrentValue();
                SelectedGroup?.OnAfterInput();
            }

            e.Handled = true;
        }

        protected override void OnSelectionChanged(RoutedEventArgs e)
        {
            if (_isSelectionChanging)
                return;

            base.OnSelectionChanged(e);

            int currentRegexGroupIndex = -1;
            for (int i = 0; i < Groups.Count; i++)
            {
                if (SelectionStart >= Groups[i].Index && SelectionStart <= Groups[i].Index + Groups[i].Length)
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
            SelectedGroup?.OnSelection();
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
                    SelectedGroup.OnDelete();
                    UpdateCurrentValue();
                }
                e.Handled = true;
            }
            // Backspace
            else if (e.Key == Key.Back)
            {
                if (SelectedGroup != null)
                {
                    SelectedGroup.OnDelete();
                    UpdateCurrentValue();
                }
                e.Handled = true;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
                ChangeSelectedGroup(1);

            if (SelectedGroup is NumericGroup numericGroup)
            {
                numericGroup.Value++;
                UpdateCurrentValue();
            }
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedGroup == null)
                ChangeSelectedGroup(1);

            if (SelectedGroup is NumericGroup numericGroup)
            {
                numericGroup.Value--;
                UpdateCurrentValue();
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var group in Groups)
                group.OnDelete();
            UpdateValues();
        }
        #endregion

        #region Input / Output

        #endregion

        #region Methods
        public void ChangeSelectedGroup(int delta)
        {
            int newindex = SelectedGroupIndex + delta;
            if (newindex < 0 || newindex >= Groups.Count)
                return;

            if (IsFocused == false)
                Focus();

            Select(Groups[newindex].Index, 0);
        }

        private void FormatText(IEnumerable<BaseGroup> groups)
        {
            // Change text and prevent selection from changing
            _isSelectionChanging = true;
            int selectionStart = SelectionStart;
            int selectionLength = SelectionLength;
            this.Text = string.Format(_outputFormat, groups.ToArray());
            Select(selectionStart, selectionLength);
            _isSelectionChanging = false;
        }

        private void UpdateCurrentValue()
        {
            if (SelectedGroup == null)
                return;

            if (Values == null)
            {
                UpdateValues();
            }
            else
            {
                object? oldValue = Values[SelectedGroupIndex];
                if (oldValue != SelectedGroup.Value)
                    UpdateValues();
            }
        }

        private void UpdateValues()
        {
            // Trigger DP change
            Values = Groups.Select(x => x.Value).ToList();
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
                    outputFormatBuilder.Append("{" + paramIndex + "}");
                    paramIndex++;
                }
                else if (groups[i] is string outputPart)
                {
                    outputFormatBuilder.Append(outputPart);
                }
            }

            _outputFormat = outputFormatBuilder.ToString();

            if (Values == null || Values.Count != Groups.Count)
                return;

            // Update group values from parts
            for (int i = 0; i < Groups.Count; i++)
            {
                Groups[i].Value = Values[i];
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
                if (depth == 0 && c == '}')
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

            if (outputFormatBuilder.Length > 0)
                groups.Add(outputFormatBuilder.ToString());

            return groups;
        }
        #endregion
    }
}
