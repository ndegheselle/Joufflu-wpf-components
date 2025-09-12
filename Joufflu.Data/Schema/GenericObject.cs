using System.Collections.ObjectModel;

namespace Joufflu.Data.Schema
{
    public interface IGenericNode
    {
        public ISchemaElement Schema { get; }
    }

    public class GenericValue : IGenericNode
    {
        public ISchemaElement Schema { get; } = new SchemaValue();
        public string ContextReference { get; set; } = string.Empty;
        public object? Value { get; set; }

        public GenericValue(ISchemaElement schema)
        {
            Schema = schema;
        }
    }

    public class GenericArray : IGenericNode
    {
        public ISchemaElement Schema { get; } = new SchemaValue() { IsArray = true };
        public ObservableCollection<IGenericNode> Values { get; set; } = [];

        public GenericArray(ISchemaElement schema)
        {
            Schema = schema;
        }
    }

    public class GenericObject : IGenericNode
    {
        public ISchemaElement Schema { get; } = new SchemaObject();
        public Dictionary<string, IGenericNode> Values { get; } = [];

        public GenericObject(ISchemaElement schema, Dictionary<string, IGenericNode> values)
        {
            Schema = schema;
            Values = values;
        }
    }
}
