using Usuel.Shared.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Joufflu.Inputs.Format
{
    public class GroupsFactory
    {
        Dictionary<string, Func<FormatTextBox, IEnumerable<string>, BaseGroup>> _types =
            new Dictionary<string, Func<FormatTextBox, IEnumerable<string>, BaseGroup>>()
        {
            { "numeric", (parent, options) => new NumericGroup(parent, options) },
            { "decimal", (parent, options) => new DecimalGroup(parent, options) },
        };

        public BaseGroup CreateParams(FormatTextBox parent, string stringParams, string? globalStringParams)
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

            throw new ArgumentException("Unknow type key.");
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

        protected readonly FormatTextBox _parent;

        public BaseGroup(FormatTextBox parent, IEnumerable<string> stringParams)
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
            foreach (var property in GetType().GetProperties())
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
                    else if (type == typeof(char) && value != null)
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

    public interface IBaseNumericGroup
    {
        public bool NoGlobalSelection { get; set; }

        public void Increment();

        public void Decrement();
    }

    public abstract class BaseNumericGroup<T> : BaseGroup, IBaseNumericGroup where T : struct
    {
        #region Options
        [Display(Name = "noGlobalSelection")]
        public bool NoGlobalSelection { get; set; } = false;

        public T? Min { get; set; }

        public T? Max { get; set; }

        public T? IncrementDelta { get; set; }

        [Display(Name = "padded")]
        public bool IsPadded { get; set; }
        #endregion

        public new T? Value
        {
            get { return (T?)base.Value; }
            set
            {
                int compareMax = Comparer<T?>.Default.Compare(value, Max);
                int compareMin = Comparer<T?>.Default.Compare(value, Min);
                if (compareMax > 0)
                {
                    base.Value = Max;
                    return;
                }
                else if (compareMin < 0)
                {
                    base.Value = Min;
                    return;
                }
                base.Value = value;
            }
        }

        public BaseNumericGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            if (NullableChar == '\0')
                NullableChar = '-';
        }

        // Should be called after the constructor
        public void Init()
        {
            if (!IsNullable)
                Value = default(T);
            if (Length == 0)
                Length = Max.ToString()!.Length;
        }

        public override bool OnInput(string input)
        {
            string newText;
            if (NoGlobalSelection)
            {
                // We replace the selected text by the input
                string oldText = _parent.Text;
                int carretOffset = 0;
                if (Index + Length < oldText.Length)
                {
                    oldText = oldText.Substring(Index, Length);
                    carretOffset = Index;
                }

                oldText = oldText.Remove(_parent.CaretIndex - carretOffset, _parent.SelectionLength);
                newText = oldText.Insert(_parent.CaretIndex - carretOffset, input);

                _parent.CaretIndex += 1;
                _parent.SelectionLength = 0;
            }
            else
            {
                newText = Value + input;
            }

            // If the number is too big we loop back to only the new number
            if (newText.Length > Length)
            {
                newText = input;
            }

            bool isValid = TryParse(newText, out T newValue);
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
            if (IsFutureValueInvalid())
                _parent.ChangeSelectedGroup(1);
            else
                OnSelection();
        }

        public override void OnSelection()
        {
            if (NoGlobalSelection)
                return;

            // For numeric groups, we select the whole number
            _parent.Select(Index, Length);
        }

        public override void OnDelete()
        {
            if (IsNullable)
                Value = null;
            else
                Value = default(T);
        }

        public override string? ToString()
        {
            if (Value == null)
                return new string(NullableChar, Length);

            string? format = Value.ToString();
            if (StringFormat != null)
                format = string.Format("{0" + StringFormat + "}", Value);
            if (IsPadded)
                format = format?.PadLeft(Length, '0');

            return format;
        }

        protected abstract bool TryParse(string newText, out T value);

        protected abstract bool IsFutureValueInvalid();

        public abstract void Increment();

        public abstract void Decrement();
    }

    public class NumericGroup : BaseNumericGroup<int>
    {
        public NumericGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            IncrementDelta = 1;

            if (Min == null)
                Min = int.MinValue;
            if (Max == null)
                Max = int.MaxValue;

            Init();
        }

        protected override bool TryParse(string newText, out int value) { return int.TryParse(newText, out value); }

        protected override bool IsFutureValueInvalid()
        {
            int futureValue = Value!.Value * 10;
            return futureValue > Max || futureValue.ToString().Length > Length;
        }

        public override void Increment()
        {
            if (Value is null)
                Value = Max;
            Value += IncrementDelta;
        }
        public override void Decrement()
        {
            if (Value is null)
                Value = Min;
            Value -= IncrementDelta;
        }
    }

    public class DecimalGroup : BaseNumericGroup<decimal>
    {
        public DecimalGroup(FormatTextBox parent, IEnumerable<string> options) : base(parent, options)
        {
            IncrementDelta = 0.1m;

            if (Min == null)
                Min = decimal.MinValue;
            if (Max == null)
                Max = decimal.MaxValue;

            Init();
        }

        protected override bool TryParse(string newText, out decimal value)
        { return decimal.TryParse(newText, out value); }

        protected override bool IsFutureValueInvalid()
        {
            decimal futureValue = Value!.Value * 10;
            return futureValue > Max || futureValue.ToString().Length > Length;
        }

        public override void Increment()
        {
            if (Value is null)
                Value = Max;
            Value += IncrementDelta;
        }
        public override void Decrement()
        {
            if (Value is null)
                Value = Min;
            Value -= IncrementDelta;
        }
    }
}