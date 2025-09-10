namespace Joufflu.Data.Schema
{
    public interface IGenericNode
    {
        public ISchemaElement Schema { get; }
    }

    public class GenericValue : IGenericNode
    {
        public ISchemaElement Schema { get; set; } = new SchemaValue();
        public string ContextReference { get; set; } = string.Empty;
        public object? Value { get; set; }
    }

    public class GenericArray : IGenericNode
    {
        public ISchemaElement Schema { get; set; } = new SchemaValue() { IsArray = true };
        public List<IGenericNode> Values { get; set; } = [];
    }

    public class GenericObject : IGenericNode
    {
        public ISchemaElement Schema { get; set; } = new SchemaObject();
        public Dictionary<string, IGenericNode> Values { get; set; } = [];
    }

}
