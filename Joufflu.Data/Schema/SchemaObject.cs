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
    {}

    public class SchemaValue : ISchemaElement
    {
        public EnumDataType DataType { get; set; }
        public bool IsArray { get; set; }
    }

    public class SchemaProperty
    {
        public string Name { get; set; }
        public ISchemaElement Element { get; set; }

        public SchemaProperty(string name, ISchemaElement element)
        {
            Name=name;
            Element=element;
        }
    }
     
    public class SchemaObject : ISchemaElement
    {
        public ObservableCollection<SchemaProperty> Properties { get; set; } = [];
        public bool IsArray { get; set; }
    }
}
