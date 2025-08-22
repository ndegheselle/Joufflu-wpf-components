namespace Usuel.Shared.Data
{
    public abstract class SchemaProperty : ISchemaProperty
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

    public class SchemaValue : SchemaProperty, ISchemaValue
    {
        public dynamic? Value { get; set; }

        public SchemaValue(string name, EnumValueType type, dynamic? value = null) : base(name, type)
        {
            Value = value;
        }

        public SchemaValue(ISchemaValue schemaValue) : base(schemaValue)
        {
            Value = schemaValue.Value;
        }
    }

    public class SchemaObject : SchemaProperty, ISchemaObject
    {
        public List<SchemaProperty> Properties { get; private set; } = [];
        IEnumerable<ISchemaProperty> ISchemaObject.Properties => Properties;

        public SchemaObject(string name) : base(name, EnumValueType.Object)
        {}

        public SchemaObject(ISchemaObject schemaObject) : base(schemaObject)
        {
            foreach (var property in schemaObject.Properties)
            {
                Properties.Add(property switch
                {
                    ISchemaValue subValue => new SchemaValue(subValue),
                    ISchemaObject subObject => new SchemaObject(subObject),
                    _ => throw new NotSupportedException($"Property type {property.GetType()} is not supported.")
                });
            }
        }
    }
}
