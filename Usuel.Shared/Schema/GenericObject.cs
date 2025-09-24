using System.Text.Json.Serialization;

namespace Usuel.Shared.Schema
{
    public enum EnumDataType
    {
        Object,
        Array,
        String,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan
    }

    public interface IGenericElement : ICloneable
    {
        IGenericParent? Parent { get; set; }
        IEnumerable<IGenericParent> ParentsTree { get; }

        new IGenericElement Clone();
        object ICloneable.Clone()
        {
            return Clone();
        }
    }

    public interface IGenericParent : IGenericElement
    {
        void Remove(object index);
    }

    public class GenericValue : IGenericElement
    {
        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public string ContextReference { get; set; } = string.Empty;
        public object Value { get; set; }
        public EnumDataType DataType { get; set; }

        public GenericValue(EnumDataType datatype, object? value = null)
        {
            DataType = datatype;
            Value = value ?? GetDefault(datatype);
        }

        public object GetDefault(EnumDataType dataType)
        {
            return dataType switch
            {
                EnumDataType.String => "",
                EnumDataType.Decimal => 0.0m,
                EnumDataType.Boolean => false,
                EnumDataType.DateTime => DateTime.Now,
                EnumDataType.TimeSpan => new TimeSpan(),
                _ => throw new NotImplementedException($"Value of type {dataType} is not handled."),
            };
        }

        public IGenericElement Clone() => new GenericValue(DataType, Value) { Parent = Parent};
    }

    public class GenericArray : IGenericParent
    {
        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public ICustomCommand AddValueCommand { get; }
        public ICustomCommand ChangeSchemaCommand { get; }

        public IGenericElement Schema { get; private set; }
        public IList<IGenericElement> Values { get; }

        public GenericArray(IGenericElement schema, IList<IGenericElement>? values = null)
        {
            Schema = schema;
            Values = values ?? [];

            ChangeSchemaCommand = new DelegateCommand<EnumDataType>(ChangeSchema);
            AddValueCommand = new DelegateCommand(() => Values.Add(Schema.Clone()));
        }

        public void ChangeSchema(EnumDataType type)
        {
            Values.Clear();
            Schema = type switch
            {
                EnumDataType.Object => new GenericObject(),
                EnumDataType.Array => new GenericArray(new GenericValue(EnumDataType.String)),
                _ => new GenericValue(type),
            };
            Schema.Parent = this;
        }

        public void Remove(object identifier)
        {
            Values.RemoveAt((int)identifier);
        }

        public IGenericElement Clone() => new GenericArray(Schema, Values.Select(x => x.Clone()).ToList()) { Parent = Parent };
    }

    public class GenericObject : IGenericParent
    {
        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public Dictionary<string, IGenericElement> Properties { get; } = [];
        
        public ICustomCommand CreatePropertyCommand { get; }

        public GenericObject(Dictionary<string, IGenericElement>? properties = null)
        {
            Properties = properties ?? new Dictionary<string, IGenericElement>() { { "Default", new GenericValue(EnumDataType.String) } };
            CreatePropertyCommand = new DelegateCommand<EnumDataType>((type) => CreateProperty("Default", type));
        }

        public void CreateProperty(string name, EnumDataType type)
        {
            IGenericElement element = type switch
            {
                EnumDataType.Object => new GenericObject(),
                EnumDataType.Array => new GenericArray(new GenericValue(EnumDataType.String)),
                _ => new GenericValue(type),
            };
            element.Parent = this;

            Properties.Add(GetUniquePropertyName(name), element);
        }

        public void Remove(object identifier)
        {
            Properties.Remove((string)identifier);
        }

        /// <summary>
        /// Gets a unique property name by appending a numeric extension if the name already exists.
        /// </summary>
        /// <param name="baseName">The base name to make unique</param>
        /// <returns>A unique property name</returns>
        private string GetUniquePropertyName(string baseName)
        {
            if (Properties.ContainsKey(baseName))
                return baseName;

            int counter = 1;
            string uniqueName;
            do
            {
                uniqueName = $"{baseName} {counter}";
                counter++;
            } while (Properties.ContainsKey(uniqueName) == false);

            return uniqueName;
        }

        public IGenericElement Clone()
        {
            Dictionary<string, IGenericElement> properties = [];
            foreach (var keyValue in Properties)
            {
                properties.Add(keyValue.Key, keyValue.Value.Clone());
            }
            return new GenericObject(properties) { Parent = Parent };
        }
    }
}