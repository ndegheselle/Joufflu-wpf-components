namespace Usuel.Shared.Data
{
    public enum EnumValueType
    {
        Dynamic,
        String,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan,
        Object
    }

    public interface ISchemaProperty
    {
        public bool IsArray { get; }
        public string Name { get; }
        public EnumValueType Type { get; }
    }

    public interface ISchemaObject : ISchemaProperty
    {
        public IEnumerable<ISchemaProperty> Properties { get; }
    }

    public class SchemaProperty : ISchemaProperty
    {
        public bool IsArray { get; set; }
        public string Name { get; set; }
        public EnumValueType Type { get; set; }

        public SchemaProperty(string name, EnumValueType type)
        {
            Name = name;
            Type = type;
        }

        public SchemaProperty(ISchemaProperty property)
        {
            Name = property.Name;
            Type = property.Type;
        }

        public override string ToString() => Name;
    }
}
