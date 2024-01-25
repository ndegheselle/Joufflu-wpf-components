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

            // Default
            return new StringGroup(parent, splitParams);
        }
    }

    public abstract class BaseGroup
    {
        protected readonly FormatedTextBox _parent;

        public int Index { get; set; } = -1;

        public int Length { get; set; } = 0;

        [Display(Name = "format")]
        public string? StringFormat { get; set; } = null;

        [Display(Name = "nullable")]
        public bool IsNullable { get; set; } = false;


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
                    else
                        throw new ArgumentException("Invalid option type : " + property.PropertyType);
                }
            }
        }

        public abstract object? Value { get; set; }

        /// <summary>
        /// What to do with the string input of the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="input"></param>
        /// <returns>Got a valid value</returns>
        public abstract bool HandleInput(string input);

        // What happen when the user click inside the group
        public abstract void HandleSelection();

        public abstract void HandleDelete();
    }

    public abstract class GenericBaseGroup<T> : BaseGroup
    {
        protected T TypedValue { get; set; }

        public override object Value { get => TypedValue; set => TypedValue = (T)value; }

        protected GenericBaseGroup(FormatedTextBox parent, IEnumerable<string> stringParams) : base(
            parent,
            stringParams)
        {
        }
    }

    public class StringGroup : GenericBaseGroup<string>
    {
        public Regex? Regex { get; set; }

        public StringGroup(FormatedTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
        }

        public override bool HandleInput(string input) { return false; }

        public override void HandleSelection()
        {
        }


        public override void HandleDelete()
        {
        }
    }

    public class NumericGroup : GenericBaseGroup<int>
    {
        public int Min { get; set; } = int.MinValue;

        public int Max { get; set; } = int.MaxValue;

        [Display(Name = "padded")]
        public bool IsPadded { get; set; }

        public NumericGroup(FormatedTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (Max != null && Length == 0)
                Length = Max.ToString().Length; // Negative max number ?

            if (IsPadded && StringFormat == null && Length != 0)
                StringFormat = $":D{Length}";
        }

        public override bool HandleInput(string input)
        {
            string newString = TypedValue + input;

            // If the number is too big we loop back to only the new number
            if (newString.Length > Length)
            {
                newString = input;
            }

            bool isValid = int.TryParse(newString, out int newValue);
            if (!isValid)
                return false;

            if (newValue > Max)
                newValue = Max;
            else if (newValue < Min)
                newValue = Min;

            TypedValue = newValue;
            return true;
        }

        public override void HandleSelection()
        {
            // For numeric groups, we select the whole number
            _parent.Select(Index, Length);
        }

        public override void HandleDelete() { Value = 0; }
    }
}