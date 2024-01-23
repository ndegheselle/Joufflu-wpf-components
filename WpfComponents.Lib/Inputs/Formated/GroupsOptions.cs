using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WpfComponents.Lib.Helpers;

namespace WpfComponents.Lib.Inputs.Formated
{
    public class GroupParamsFactory
    {
        Dictionary<string, Func<IEnumerable<string>, BaseGroupParams>> _types = 
            new Dictionary<string, Func<IEnumerable<string>, BaseGroupParams>>()
        { { "numeric", (options) => new NumericParams(options) } };

        public BaseGroupParams CreateParams(string stringParams, string? globalStringParams)
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
                    return type.Value.Invoke(splitParams);
                }
            }

            return new BaseGroupParams(splitParams);
        }
    }

    public class BaseGroupParams
    {
        public int? Length { get; set; }

        [Display(Name = "format")]
        public string? StringFormat { get; set; } = null;

        public Regex? Regex { get; set; } = null;
        [Display(Name = "nullable")]
        public bool IsNullable { get; set; } = false;

        #region Init
        public BaseGroupParams(IEnumerable<string> stringParams)
        {
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

            Build();
        }

        // Make the different properties work together
        public virtual void Build()
        {
            // Create the regex
            if (Regex != null)
                return;

            string inputLength = (Length > 0) ? "{1," + Length + "}" : "+";
            if (IsNullable)
                Regex = new Regex(@$"(\s{inputLength}|{BuildRegex(inputLength)})");
            else
                Regex = new Regex(@$"({BuildRegex(inputLength)})");
        }

        public virtual string BuildRegex(string inputLength) { return @"\w" + inputLength; }
        #endregion

        // What 
        public virtual void HandleInput(string input)
        {
        }

        // What happen when the user click inside the group
        public virtual void HandleSelection()
        {
        }

        // After the regex apply logic on the value of the group
        public virtual void HandleValidation()
        {
        }
    }

    public class NumericParams : BaseGroupParams
    {
        public int? Min { get; set; }

        public int? Max { get; set; }

        [Display(Name = "padded")]
        public bool IsPadded { get; set; }

        #region Init
        public NumericParams(IEnumerable<string> options) : base(options)
        {
        }

        public override void Build()
        {
            if (Max != null && Length == null)
                Length = Max.ToString().Length; // Negative max number ?

            if (IsPadded && StringFormat == null && Length != null)
                StringFormat = $":D{Length}";

            base.Build();
        }

        public override string BuildRegex(string inputLength) { return @"\d" + inputLength; }
        #endregion
    }
}
