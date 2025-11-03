using System.ComponentModel;

namespace Joufflu.Data.Shared
{
    /// <summary>
    /// Element that store a primitive value.
    /// </summary>
    public class GenericValue : GenericElement, INotifyPropertyChanged
    {
        public object Value { get; set; }
        public EnumDataType DataType { get; set; }

        public GenericValue(EnumDataType datatype, object? value = null)
        {
            DataType = datatype;
            Value = value ?? GetDefault(datatype);
        }

        #region Contexte

        public override void ApplyContext(Dictionary<string, GenericReference> contextReferences, int depth = 0)
        {
            if (string.IsNullOrEmpty(ContextReference))
                return;

            // Handle nested context references
            if (depth > 200)
                throw new InvalidOperationException($"More than 200 nested context references detected for '{ContextReference}'.");

            if (contextReferences.TryGetValue(ContextReference.Trim(), out GenericReference? reference) == false)
                throw new ArgumentException($"Could not find '{ContextReference}' in context.");

            if (reference.Element is not GenericValue valueElement)
                throw new Exception($"The element '{ContextReference}' is not a value element.");

            valueElement.ApplyContext(contextReferences, depth+1);
            Value = valueElement.Value;
        }

        #endregion

        public override GenericElement Clone() => new GenericValue(DataType, Value) { Parent = Parent };

        public override string? ToString()
        {
            return DataType switch
            {
                EnumDataType.Enum or
                EnumDataType.String or
                EnumDataType.Decimal or
                EnumDataType.Integer => Value.ToString(),
                EnumDataType.Boolean => (bool)Value ? "True" : "False",
                EnumDataType.DateTime => ((DateTime)Value).ToString("yyyy/MM/dd HH:mm"),
                EnumDataType.TimeSpan => ((TimeSpan)Value).ToString(@"dd\:hh\:mm\:ss"),
                _ => throw new NotImplementedException($"Value of type {DataType} is not handled."),
            };
        }

        /// <summary>
        /// Get default value based on a EnumDataType.
        /// </summary>
        /// <param name="dataType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static object GetDefault(EnumDataType dataType)
        {
            return dataType switch
            {
                EnumDataType.String => "",
                EnumDataType.Decimal => 0.0m,
                EnumDataType.Integer => 0,
                EnumDataType.Boolean => false,
                EnumDataType.DateTime => DateTime.Now,
                EnumDataType.TimeSpan => TimeSpan.Zero,
                _ => throw new NotImplementedException($"Value of type {dataType} is not handled."),
            };
        }
    }

    /// <summary>
    /// Enum value with index and name.
    /// </summary>
    public class GenericEnumValue
    {
        public int Index { get; private set; }
        public string Name { get; private set; }

        public GenericEnumValue(int index, string name)
        {
            Index = index;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Element that represent an enum value.
    /// </summary>
    public class GenericEnum : GenericValue
    {
        /// <summary>
        /// List of availables enum values.
        /// </summary>
        public IEnumerable<GenericEnumValue> Availables { get; set; } = [];
        public GenericEnum(IEnumerable<GenericEnumValue> availables, int value = 0) : base(EnumDataType.Enum, value)
        {
            Availables = availables;
        }

        public GenericEnum(int value = 0) : base(EnumDataType.Enum, value)
        {}

        public override GenericElement Clone() => new GenericEnum(Availables, Value as int? ?? 0) { Parent = Parent };
    }
}