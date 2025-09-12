using System.Collections.ObjectModel;

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
        IGenericNode ToValue();
    }

    public class SchemaValue : ISchemaElement
    {
        public EnumDataType DataType { get; set; }
        public bool IsArray { get; set; }

        public IGenericNode ToValue()
        {
            return IsArray ? new GenericArray(this) : new GenericValue(this);
        }
    }

    public class SchemaProperty
    {
        public string Name { get; set; }
        public ISchemaElement Element { get; set; }
        public uint Depth { get; set; }

        public SchemaObject Parent { get; set; }
        public bool IsSelected { get; set; }

        public SchemaProperty(SchemaObject parent, string name, ISchemaElement element)
        {
            Parent = parent;
            Name = name;
            Element = element;
        }
    }
     
    public class SchemaObject : ISchemaElement
    {
        public ObservableCollection<SchemaProperty> Properties { get; set; } = [];
        public bool IsArray { get; set; }

        public SchemaObject Add(string name, ISchemaElement element, uint depth = 0)
        {
            Properties.Add(new SchemaProperty(this, name, element) { Depth = depth });
            return this;
        }

        public IGenericNode ToValue()
        {
            Dictionary<string, IGenericNode> values = Properties.ToDictionary(
                prop => prop.Name, 
                prop => prop.Element.ToValue());

            return IsArray ? new GenericArray(this) : new GenericObject(this, values);
        }
    }
}
