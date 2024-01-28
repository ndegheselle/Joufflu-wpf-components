using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using WpfComponents.Lib.Helpers;

namespace WpfComponents.Lib.Inputs.Formated
{
    public class GroupsFactory
    {
        Dictionary<string, Func<FormatedTextBox, IEnumerable<string>, BaseGroup>> _types = 
            new Dictionary<string, Func<FormatedTextBox, IEnumerable<string>, BaseGroup>>()
        { { "numeric", (parent, options) => new NumericGroup(parent, options) } };

        public BaseGroup CreateParams(FormatedTextBox parent, string stringParams, string? globalStringParams)
        {
            IEnumerable<string> splitParams = stringParams.Split("|");
            if (splitParams.Count() <= 0)
                throw new ArgumentException("Options can not be empty.");

            // Add global options at the beginning
            if (globalStringParams != null)
                splitParams = globalStringParams.Split("|").Concat(splitParams);

            // For each _types, check if options contains it
            // If yes, remove it from options and create the type
            // If no, create the default type
            foreach (var type in _types)
            {
                if (splitParams.Contains(type.Key))
                {
                    splitParams = splitParams.Where(x => x != type.Key);
                    return type.Value.Invoke(parent, splitParams);
                }
            }

            return null;
            // May use a StringGroup
            // return new StringGroup(parent, splitParams);
        }
    }

    public abstract class BaseGroup
    {
        #region Options
        public int Length { get; set; } = 0;

        [Display(Name = "format")]
        public string? StringFormat { get; set; } = null;

        [Display(Name = "nullable")]
        public bool IsNullable { get; set; } = false;

        public char NullableChar { get; set; }
        #endregion

        public int Index { get; set; } = -1;

        public object? Value { get; set; }

        protected readonly FormatedTextBox _parent;

        public BaseGroup(FormatedTextBox parent, IEnumerable<string> stringParams)
        {
            _parent = parent;
            // Separate key and value
            Dictionary<string, string?> paramKeyValue = new Dictionary<string, string?>();
            foreach (var param in stringParams)
            {
                string[] splitParam = param.Split(":", 2);

                string key = splitParam[0];

                string? value = null;
                if (splitParam.Length > 1)
                    value = splitParam[1];

                paramKeyValue.Add(key, value);
            }

            // For each property, check if options contains it
            // XXX : may want to create a separate class options and only look in it
            foreach (var property in this.GetType().GetProperties())
            {
                // Get display name attribute
                var displayAttribute = property.GetCustomAttribute<DisplayAttribute>();
                string displayName = displayAttribute?.Name ?? property.Name.FirstCharToLowerCase();

                // Set properties
                if (paramKeyValue.ContainsKey(displayName))
                {
                    Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    string? value = paramKeyValue[displayName];
                    if (type == typeof(int) && value != null)
                        property.SetValue(this, int.Parse(value));
                    else if (type == typeof(string) && value != null)
                        property.SetValue(this, value);
                    else if (type == typeof(Regex) && value != null)
                        property.SetValue(this, new Regex(value));
                    else if (type == typeof(bool))
                        property.SetValue(this, true);
                    else if (type == typeof(char))
                        property.SetValue(this, value[0]);
                    else
                        throw new ArgumentException("Invalid option type : " + property.PropertyType);
                }
            }
        }

        /// <summary>
        /// What to do with the string input of the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="input"></param>
        /// <returns>Got a valid value</returns>
        public abstract bool OnInput(string input);

        public abstract void OnAfterInput();

        // What happen when the user click inside the group
        public abstract void OnSelection();

        public abstract void OnDelete();
    }

    public class NumericGroup : BaseGroup
    {
        #region Options
        public int Min { get; set; } = int.MinValue;

        public int Max { get; set; } = int.MaxValue;

        [Display(Name = "padded")]
        public bool IsPadded { get; set; }
        #endregion

        public new int? Value
        {
            get { return (int?)base.Value; }
            set
            {
                if (value > Max)
                {
                    base.Value = Max;
                    return;
                }
                else if (value < Min)
                {
                    base.Value = Min;
                    return;
                }
                base.Value = value;
            }
        }

        public NumericGroup(FormatedTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (NullableChar == '\0')
                NullableChar = '-';

            if (Max != int.MaxValue && Length == 0)
                Length = Max.ToString().Length; // Negative max number ?

            if (IsPadded && StringFormat == null && Length != 0)
                StringFormat = $":D{Length}";
        }

        public override bool OnInput(string input)
        {
            string newString = Value + input;

            // If the number is too big we loop back to only the new number
            if (newString.Length > Length)
            {
                newString = input;
            }

            bool isValid = int.TryParse(newString, out int newValue);
            if (!isValid)
                return false;

            Value = newValue;
            return true;
        }

        public override void OnAfterInput()
        {
            if (Value == null)
                return;

            // If the next input will make the number too big, we change group
            int futureValue = Value.Value * 10;
            if (futureValue > Max || futureValue.ToString().Length > Length)
                _parent.ChangeSelectedGroup(1);
        }

        public override void OnSelection()
        {
            // For numeric groups, we select the whole number
            _parent.Select(Index, Length);
        }

        public override void OnDelete()
        {
            if (IsNullable)
                Value = null;
            else
                Value = 0;
        }

        public override string ToString()
        {
            if (Value == null)
                return new string(NullableChar, Length);
            return string.Format("{0" + StringFormat + "}", Value);
        }
    }
}