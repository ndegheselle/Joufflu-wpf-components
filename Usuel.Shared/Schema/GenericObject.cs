using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Usuel.Shared.Schema
{
    public enum EnumDataType
    {
        Object,
        Array,
        Enum,
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

    public interface IGenericParent : IGenericElement, INotifyPropertyChanged
    {
        IEnumerable<GenericProperty> Childrens { get; }

        /// <summary>
        /// Change an identifier to a new value.
        /// </summary>
        /// <param name="oldIdentifier"></param>
        /// <param name="newIdentifier"></param>
        /// <returns>Return false if the newIdentifier is already used.</returns>
        bool ChangeIdentifier(object oldIdentifier, object newIdentifier);
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

    public class GenericProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public object Identifier { get; set; }
        public IGenericElement Element { get; }
        public ICustomCommand RemoveCommand { get; }

        public bool IsRemovable { get; set; } = true;
        public bool IsIdentifierEditable { get; set; } = true;

        public GenericProperty(object identifier, IGenericElement element)
        {
            Identifier = identifier;
            Element = element;
            RemoveCommand = new DelegateCommand(() => Element.Parent?.Remove(Identifier), () => IsRemovable);
        }

        private void NotifypropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class GenericArray : IGenericParent
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];
        [JsonIgnore]
        public IEnumerable<GenericProperty> Childrens => [
                    new GenericProperty("Schema", Schema) { IsRemovable = false, IsIdentifierEditable = false },
                    ..Values.Select((val, index) => new GenericProperty(index, val) { IsIdentifierEditable = false })
                ];

        public ICustomCommand AddValueCommand { get; }
        public ICustomCommand ChangeSchemaCommand { get; }

        public IGenericElement Schema { get; private set; }
        public IList<IGenericElement> Values { get; }

        public GenericArray(IGenericElement schema, IList<IGenericElement>? values = null)
        {
            Schema = schema;
            Values = values ?? [];

            ChangeSchemaCommand = new DelegateCommand<EnumDataType>(ChangeSchema);
            AddValueCommand = new DelegateCommand(Add);
            Schema.Parent = this;
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
            NotifypropertyChanged(nameof(Childrens));
        }

        public void Add()
        {
            Values.Add(Schema.Clone());
            NotifypropertyChanged(nameof(Childrens));
        }

        public void Remove(object identifier)
        {
            Values.RemoveAt((int)identifier);
            NotifypropertyChanged(nameof(Childrens));
        }

        public IGenericElement Clone() => new GenericArray(Schema, Values.Select(x => x.Clone()).ToList()) { Parent = Parent };

        private void NotifypropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public bool ChangeIdentifier(object oldIdentifier, object newIdentifier)
        {
            throw new NotImplementedException();
        }
    }

    public class GenericObject : IGenericParent
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];
        [JsonIgnore]
        public IEnumerable<GenericProperty> Childrens => Properties.Select(x => new GenericProperty(x.Key, x.Value));

        public Dictionary<string, IGenericElement> Properties { get; }
        public ICustomCommand CreatePropertyCommand { get; }

        public GenericObject(Dictionary<string, IGenericElement>? properties = null)
        {
            Properties = properties ?? [];
            CreatePropertyCommand = new DelegateCommand<EnumDataType>((type) => CreateProperty("Default", type));

            if (Properties.Count == 0)
                CreateProperty("Default", EnumDataType.String);
        }

        public void AddProperty(string name, IGenericElement element)
        {
            element.Parent = this;
            Properties.Add(name, element);
            NotifypropertyChanged(nameof(Childrens));
        }

        public void CreateProperty(string name, EnumDataType type)
        {
            IGenericElement element = type switch
            {
                EnumDataType.Object => new GenericObject(),
                EnumDataType.Array => new GenericArray(new GenericValue(EnumDataType.String)),
                _ => new GenericValue(type),
            };
            AddProperty(GetUniquePropertyName(name), element);
        }

        public void Remove(object identifier)
        {
            Properties.Remove((string)identifier);
            NotifypropertyChanged(nameof(Childrens));
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

        public bool ChangeIdentifier(object oldIdentifier, object newIdentifier)
        {
            if (Properties.ContainsKey((string)newIdentifier))
                return false;

            Properties[(string)newIdentifier] = Properties[(string)oldIdentifier];
            Properties.Remove((string)oldIdentifier);
            return true;
        }

        /// <summary>
        /// Gets a unique property name by appending a numeric extension if the name already exists.
        /// </summary>
        /// <param name="baseName">The base name to make unique</param>
        /// <returns>A unique property name</returns>
        private string GetUniquePropertyName(string baseName)
        {
            if (Properties.ContainsKey(baseName) == false)
                return baseName;

            int counter = 1;
            string uniqueName;
            do
            {
                uniqueName = $"{baseName} {counter}";
                counter++;
            } while (Properties.ContainsKey(uniqueName));

            return uniqueName;
        }

        private void NotifypropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}