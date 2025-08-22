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

    public interface ISchemaValue : ISchemaProperty
    {
        public dynamic? Value { get; }
    }

    public interface ISchemaObject : ISchemaProperty
    {
        public IEnumerable<ISchemaProperty> Properties { get; }
    }
}
