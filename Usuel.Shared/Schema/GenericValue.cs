using System.ComponentModel;

namespace Usuel.Shared.Schema
{
    public enum EnumDataType
    {
        Object,
        Array,
        Enum,
        String,
        Integer,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan
    }

    /// <summary>
    /// Eelement with childrens.
    /// </summary>
    public interface IGenericParent : IGenericElement, INotifyPropertyChanged
    {
        /// <summary>
        /// List of the properties of the schema of the parent.
        /// </summary>
        IEnumerable<GenericProperty> SchemaProperties { get; }
        /// <summary>
        /// Liste of the values of the parent (can be identical to <see cref="SchemaProperties"/>).
        /// </summary>
        IEnumerable<GenericProperty> ValuesProperties { get; }

        /// <summary>
        /// Change an identifier to a new value.
        /// </summary>
        /// <param name="oldIdentifier"></param>
        /// <param name="newIdentifier"></param>
        /// <returns>Return false if the newIdentifier is already used.</returns>
        bool ChangeIdentifier(object oldIdentifier, object newIdentifier);

        /// <summary>
        /// Remove an item with a specific identifier.
        /// </summary>
        /// <param name="identifier"></param>
        void Remove(object identifier);
    }

    /// <summary>
    /// Reference to another element (identifier can be nested with comma separated).
    /// </summary>
    public class GenericReference
    {
        public string Identifier { get; set; }
        public EnumDataType DataType { get; set; }

        public GenericReference(string identifier, EnumDataType dataType)
        {
            Identifier = identifier;
            DataType = dataType;
        }

        public override string ToString() => Identifier;

        public override bool Equals(object? obj)
        {
            return (obj is GenericReference other) ? string.Equals(Identifier, other.Identifier) : false;
        }

        public override int GetHashCode() => Identifier.GetHashCode();
    }

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

        public override GenericElement Clone() => new GenericValue(DataType, Value) { Parent = Parent };

        public override string? ToString()
        {
            return DataType switch
            {
                EnumDataType.String or
                EnumDataType.Decimal or
                EnumDataType.Integer => Value.ToString(),
                EnumDataType.Boolean => (bool)Value ? "True" : "False",
                EnumDataType.DateTime => ((DateTime)Value).ToString("yyyy/MM/dd HH:mm"),
                EnumDataType.TimeSpan => ((TimeSpan)Value).ToString("d:hh:mm:ss"),
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
                EnumDataType.TimeSpan => new TimeSpan(),
                _ => throw new NotImplementedException($"Value of type {dataType} is not handled."),
            };
        }
    }

    /// <summary>
    /// Element that represent an enum value.
    /// </summary>
    public class GenericEnum : GenericValue
    {
        /// <summary>
        /// Enum value with index and name.
        /// </summary>
        public class EnumValue
        {
            public int Index { get; private set; }
            public string Name { get; private set; }

            public EnumValue(int index, string name)
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
        /// List of availables enum values.
        /// </summary>
        public IEnumerable<EnumValue> Availables { get; set; }

        public GenericEnum(IEnumerable<EnumValue> availables, int value = 0) : base(EnumDataType.Enum, value)
        {
            Availables = availables;
        }

        public override GenericElement Clone() => new GenericEnum(Availables, Value as int? ?? 0) { Parent = Parent };
        public override string ToString()
        {
            return string.Join(", ", Availables);
        }
    }
}