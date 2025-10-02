using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Usuel.Shared.Schema
{
    public enum EnumDataType
    {
        Object,
        Array,
        Enum,
        String,
        Integer,
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
        IEnumerable<GenericProperty> SchemaProperties { get; }
        IEnumerable<GenericProperty> ValuesProperties { get; }

        /// <summary>
        /// Change an identifier to a new value.
        /// </summary>
        /// <param name="oldIdentifier"></param>
        /// <param name="newIdentifier"></param>
        /// <returns>Return false if the newIdentifier is already used.</returns>
        bool ChangeIdentifier(object oldIdentifier, object newIdentifier);
        void Remove(object index);
    }

    public class GenericReference
    {
        public string Identifier { get; set; }
        public EnumDataType DataType { get; set; }

        public GenericReference(string identifier, EnumDataType dataType)
        {
            Identifier=identifier;
            DataType=dataType;
        }

        public override string ToString() => Identifier;
    }

    public class GenericValue : IGenericElement, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public string ContextReference { get; set; } = string.Empty;
        public object Value { get; set; }
        public EnumDataType DataType { get; set; }

        public ICommand ClearReferenceCommand { get; private set; }

        public GenericValue(EnumDataType datatype, object? value = null)
        {
            DataType = datatype;
            Value = value ?? GetDefault(datatype);

            ClearReferenceCommand = new DelegateCommand<bool>((clear) =>
            {
                if (clear)
                    ContextReference = string.Empty;
            });
        }

        public virtual IGenericElement Clone() => new GenericValue(DataType, Value) { Parent = Parent};

        public override string? ToString()
        {
            return DataType switch
            {
                EnumDataType.String or
                EnumDataType.Decimal or
                EnumDataType.Integer => Value.ToString(),
                EnumDataType.Boolean => (bool)Value ? "True" : "False",
                EnumDataType.DateTime => ((DateTime)Value).ToString("yyyy/MM/dd HH:mm"),
                EnumDataType.TimeSpan => ((TimeSpan)Value).ToString("d:hh:mm:ss"),
                _ => throw new NotImplementedException($"Value of type {DataType} is not handled."),
            };
        }

        public static object GetDefault(EnumDataType dataType)
        {
            return dataType switch
            {
                EnumDataType.String => "",
                EnumDataType.Decimal => 0.0m,
                EnumDataType.Integer => 0,
                EnumDataType.Boolean => false,
                EnumDataType.DateTime => DateTime.Now,
                EnumDataType.TimeSpan => new TimeSpan(),
                _ => throw new NotImplementedException($"Value of type {dataType} is not handled."),
            };
        }
    }

    public class GenericEnum : GenericValue
    {
        public class EnumValue
        {
            public int Index { get; private set; }
            public string Name { get; private set; }

            public EnumValue(int index, string name)
            {
                Index = index;
                Name = name;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public IEnumerable<EnumValue> Availables { get; set; }
        public GenericEnum(IEnumerable<EnumValue> availables, int value = 0) : base(EnumDataType.Enum, value)
        {
            Availables = availables;
        }

        public override IGenericElement Clone() => new GenericEnum(Availables, Value as int? ?? 0) { Parent = Parent };
        public override string ToString()
        {
            return string.Join(", ", Availables);
        }
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
        public IEnumerable<GenericProperty> SchemaProperties => [new GenericProperty("Schema", Schema) { IsRemovable = false, IsIdentifierEditable = false }];
        [JsonIgnore]
        public IEnumerable<GenericProperty> ValuesProperties => Values.Select((val, index) => new GenericProperty(index, val) { IsIdentifierEditable = false });

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
            foreach (var value in Values)
                value.Parent = this;
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
            NotifypropertyChanged(nameof(SchemaProperties));
        }

        public void Add()
        {
            Values.Add(Schema.Clone());
            NotifypropertyChanged(nameof(ValuesProperties));
        }

        public void Remove(object identifier)
        {
            Values.RemoveAt((int)identifier);
            NotifypropertyChanged(nameof(ValuesProperties));
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
        public IEnumerable<GenericProperty> SchemaProperties => Properties.Select(x => new GenericProperty(x.Key, x.Value));
        [JsonIgnore]
        public IEnumerable<GenericProperty> ValuesProperties => SchemaProperties;


        public Dictionary<string, IGenericElement> Properties { get; }
        public ICustomCommand CreatePropertyCommand { get; }

        public GenericObject(Dictionary<string, IGenericElement>? properties = null)
        {
            Properties = properties ?? [];
            CreatePropertyCommand = new DelegateCommand<EnumDataType>((type) => CreateProperty("Default", type));

            if (properties == null)
                CreateProperty("Default", EnumDataType.String);
        }

        public void AddProperty(string name, IGenericElement element)
        {
            element.Parent = this;
            Properties.Add(name, element);
            NotifypropertyChanged(nameof(SchemaProperties));
            NotifypropertyChanged(nameof(ValuesProperties));
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
            NotifypropertyChanged(nameof(SchemaProperties));
            NotifypropertyChanged(nameof(ValuesProperties));
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

        public IEnumerable<GenericReference> GetReferences(IEnumerable<string>? parentIdentifiers = null)
        {
            parentIdentifiers = parentIdentifiers ?? [];
            List<GenericReference> references = new List<GenericReference>();
            foreach(var property in Properties)
            {
                var identifiers = parentIdentifiers.Append(property.Key);
                string identifier = string.Join(".", identifiers);
                if (property.Value is GenericObject @object)
                {
                    references.AddRange(@object.GetReferences(identifiers));
                }
                else if (property.Value is GenericArray array)
                {
                    references.Add(new GenericReference(identifier, EnumDataType.Array));
                }
                else if (property.Value is GenericValue value)
                {
                    references.Add(new GenericReference(identifier, value.DataType));
                }
            }
            return references;
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