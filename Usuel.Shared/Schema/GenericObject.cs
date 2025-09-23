using System.Collections.ObjectModel;

namespace Usuel.Shared.Schema
{
    public interface IGenericElement
    {
        ISchemaElement? Schema { get; }
    }
    public class GenericNode<TSchema> : IGenericElement where TSchema : ISchemaElement
    {
        ISchemaElement? IGenericElement.Schema => Schema;
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
                EnumDataType.Decimal => 0.0m,
                EnumDataType.Boolean => false,
                EnumDataType.DateTime => DateTime.Now,
                EnumDataType.TimeSpan => new TimeSpan(),
                _ => null
            };
        }
    }

    public class GenericArray : GenericNode<SchemaArray>
    {
        public ObservableCollection<IGenericElement> Values { get; set; } = [];

        public ICustomCommand AddCommand { get; set; }
        public ICustomCommand RemoveCommand { get; set; }

        public GenericArray(SchemaArray schema)
        {
            Schema = schema;
            AddCommand = new DelegateCommand(() => Values.Add(Schema.Type.Element.ToValue()));
            RemoveCommand = new DelegateCommand(() => Values.RemoveAt(Values.Count - 1));
        }
    }

    public class GenericObject : GenericNode<SchemaObject>
    {
        public Dictionary<string, IGenericElement> Properties { get; } = [];

        public GenericObject(SchemaObject schema, Dictionary<string, IGenericElement> properties)
        {
            Schema = schema;
            Properties = properties;
        }
    }
}