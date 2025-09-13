using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Joufflu.Data.Schema
{
    public enum EnumDataType
    {
        Dynamic = 0,
        String,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan
    }

    public interface ISchemaElement
    {
        bool IsArray { get; }
        IGenericNode ToValue();
    }

    public interface ISubSchemaElement : ISchemaElement
    {
        string? Name { get; set; }
        SchemaObject? Parent { get; set; }
        bool IsSelected { get; }
    }

    public class SchemaValue : ISubSchemaElement
    {
        public string? Name { get; set; }
        public EnumDataType DataType { get; set; }
        public bool IsArray { get; set; }

        [JsonIgnore]
        public SchemaObject? Parent { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public IGenericNode ToValue()
        {
            return IsArray ? new GenericArray(this) : new GenericValue(this);
        }
    }
         
    public class SchemaObject : ISubSchemaElement
    {
        public string? Name { get; set; }
        public bool IsArray { get; set; }

        [JsonIgnore]
        public SchemaObject? Parent { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public ObservableCollection<ISubSchemaElement> Properties { get; set; } = [];

        public SchemaObject Add(string name, ISubSchemaElement element)
        {
            element.Parent = this;
            element.Name = name;
            Properties.Add(element);
            return this;
        }

        public IGenericNode ToValue()
        {
            Dictionary<string, IGenericNode> values = Properties.ToDictionary(
                prop => prop.Name ?? "",
                prop => prop.ToValue());

            return IsArray ? new GenericArray(this) : new GenericObject(this, values);
        }
    }
}
