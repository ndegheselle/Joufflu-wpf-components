using System.Collections.ObjectModel;

namespace Joufflu.Data.Schema
{
    public interface IGenericNode
    {
        ISubSchemaElement? Schema { get; }
    }
    public class GenericNode<TSchema> : IGenericNode where TSchema : ISubSchemaElement
    {
        ISubSchemaElement? IGenericNode.Schema => Schema;
        public TSchema? Schema { get; protected set; }
    }

    public class GenericValue : GenericNode<SchemaValue>
    {
        public string ContextReference { get; set; } = string.Empty;
        public object? Value { get; set; }

        public GenericValue(SchemaValue schema)
        {
            Schema = schema;
            Value = GetDefault(schema.DataType);
        }

        public object? GetDefault(EnumDataType dataType)
        {
            return dataType switch
            {
                EnumDataType.String => "",
                EnumDataType.Decimal => 0.0,
                EnumDataType.Boolean => false,
                EnumDataType.DateTime => DateTime.Now,
                EnumDataType.TimeSpan => new TimeSpan(),
                _ => null
            };
        }
    }

    public class GenericArray : IGenericNode
    {
        public ISubSchemaElement Schema { get; } = new SchemaValue() { IsArray = true };
        public ObservableCollection<IGenericNode> Values { get; set; } = [];

        public GenericArray(ISubSchemaElement schema)
        {
            Schema = schema;
        }
    }

    public class GenericObject : GenericNode<SchemaObject>
    {
        public Dictionary<string, IGenericNode> Properties { get; } = [];

        public GenericObject(SchemaObject schema, Dictionary<string, IGenericNode> properties)
        {
            Schema = schema;
            Properties = properties;
        }
    }
}