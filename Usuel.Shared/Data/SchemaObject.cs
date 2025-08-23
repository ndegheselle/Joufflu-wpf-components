namespace Usuel.Shared.Data
{
    public class SchemaObject : SchemaProperty, ISchemaObject
    {
        public List<SchemaProperty> Properties { get; private set; } = [];
        IEnumerable<ISchemaProperty> ISchemaObject.Properties => Properties;

        public SchemaObject(string name) : base(name, EnumValueType.Object)
        { }

        public SchemaObject(ISchemaObject schemaObject) : base(schemaObject)
        {
            foreach (var property in schemaObject.Properties)
            {
                Properties.Add(property switch
                {
                    ISchemaObject subObject => new SchemaObject(subObject),
                    ISchemaProperty subProperty => new SchemaProperty(subProperty),
                    _ => throw new NotSupportedException($"Property type {property.GetType()} is not supported.")
                });
            }
        }
    }
}
