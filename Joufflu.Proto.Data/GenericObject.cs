using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Usuel.Shared;

namespace Joufflu.Proto.Data
{
    public interface IGenericIdentifier
    {
        public GenericElement Element { get; }
        public ICustomCommand? RemoveCommand { get; }
    }

    /// <summary>
    /// Generic identifier that can either be a property or index.
    /// </summary>
    /// <typeparam name="TIdentifier"></typeparam>
    public class GenericIdentifier<TIdentifier> : IGenericIdentifier, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public TIdentifier Identifier { get; set; }
        public GenericElement Element { get; }
        public ICustomCommand? RemoveCommand { get; protected set; }

        public GenericIdentifier(TIdentifier identifier, GenericElement element)
        {
            Identifier = identifier;
            Element = element;
        }

        protected void NotifypropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string? ToString() => Identifier?.ToString();

        public override bool Equals(object? obj)
        {
            return obj is GenericReference other && string.Equals(Identifier, other.Identifier);
        }
        public override int GetHashCode() => Identifier!.GetHashCode();
    }

    /// <summary>
    /// Property of a <see cref="GenericObject"/>.
    /// </summary>
    public class GenericProperty : GenericIdentifier<string>
    {
        public GenericObject Parent => Element.Parent as GenericObject ?? throw new Exception("The property should have a Object parent.");

        public bool IsIdentifierEditable { get; set; } = true;
        public bool IsRemovable { get; set; } = true;

        public GenericProperty(string identifier, GenericElement element) : base(identifier, element)
        {
            RemoveCommand = new DelegateCommand(() => Parent.Remove(Identifier), () => IsRemovable);
        }

        public bool Rename(string newName)
        {
            // Check if unique
            if (Parent.DoesPropertyExist(newName) == false)
            {
                return false;
            }
            Parent.RenameProperty(Identifier, newName);
            Identifier = newName;
            return true;
        }
    }

    /// <summary>
    /// Index of a <see cref="GenericArray"/>.
    /// </summary>
    public class GenericIndex : GenericIdentifier<int>
    {
        public GenericArray Parent => Element.Parent as GenericArray ?? throw new Exception("The index should have a Array parent.");

        public GenericIndex(int identifier, GenericElement element) : base(identifier, element)
        {
            RemoveCommand = new DelegateCommand(() => Parent.RemoveAt(Identifier));
        }
    }

    /// <summary>
    /// Array of <see cref="GenericElement"/>.
    /// </summary>
    public class GenericArray : GenericElement, IGenericParent
    {
        // XXX : Having a GenericProperty in a GenericArray can cause problem if the user try to remove the element
        [JsonIgnore]
        public IEnumerable<GenericProperty> MetadataProperties => [new GenericProperty("Schema", Schema) { IsRemovable = false, IsIdentifierEditable = false }];
        [JsonIgnore]
        public IEnumerable<GenericIndex> IndexedValues => Values.Select((val, index) => new GenericIndex(index, val));

        public ICustomCommand AddValueCommand { get; }
        public ICustomCommand ChangeSchemaCommand { get; }

        /// <summary>
        /// Schema that represent the kind of data that is stored in the values.
        /// </summary>
        public GenericElement Schema { get; private set; }
        public IList<GenericElement> Values { get; }

        public GenericArray(GenericElement schema, IList<GenericElement>? values = null)
        {
            Schema = schema;
            Values = values ?? [];

            ChangeSchemaCommand = new DelegateCommand<EnumDataType>(ChangeSchema);
            AddValueCommand = new DelegateCommand(CreateValue);
            Schema.Parent = this;
            foreach (var value in Values)
                value.Parent = this;
        }

        /// <summary>
        /// Change the schema of the array.
        /// </summary>
        /// <param name="type"></param>
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
            NotifypropertyChanged(nameof(MetadataProperties));
        }

        /// <summary>
        /// Create a value based on the schema.
        /// </summary>
        public void CreateValue()
        {
            Values.Add(Schema.Clone());
            NotifypropertyChanged(nameof(IndexedValues));
        }

        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
            NotifypropertyChanged(nameof(IndexedValues));
        }

        public override GenericElement Clone() => new GenericArray(Schema, Values.Select(x => x.Clone()).ToList()) { Parent = Parent };

        public override void ApplyContext(Dictionary<string, GenericReference> contextReferences, int depth = 0)
        {
            foreach (var value in Values)
            {
                value.ApplyContext(contextReferences, depth);
            }
        }
    }

    /// <summary>
    /// Object composed of properties of <see cref="GenericElement"/>.
    /// </summary>
    public class GenericObject : GenericElement, IGenericParent
    {
        [JsonIgnore]
        public IEnumerable<GenericProperty> ValuesProperties => Properties.Select(x => new GenericProperty(x.Key, x.Value));

        public Dictionary<string, GenericElement> Properties { get; }
        public ICustomCommand CreatePropertyCommand { get; }

        public GenericObject(Dictionary<string, GenericElement>? properties = null)
        {
            Properties = properties ?? [];
            CreatePropertyCommand = new DelegateCommand<EnumDataType>((type) => CreateProperty("Default", type));

            if (properties == null)
                CreateProperty("Default", EnumDataType.String);
        }

        public void AddProperty(string name, GenericElement element)
        {
            element.Parent = this;
            Properties.Add(name, element);
            NotifypropertyChanged(nameof(ValuesProperties));
        }

        /// <summary>
        /// Create a property from a specific <see cref="EnumDataType"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public void CreateProperty(string name, EnumDataType type)
        {
            GenericElement element = type switch
            {
                EnumDataType.Object => new GenericObject(),
                EnumDataType.Array => new GenericArray(new GenericValue(EnumDataType.String)),
                _ => new GenericValue(type),
            };
            AddProperty(GetUniquePropertyName(name), element);
        }

        public override GenericElement Clone()
        {
            Dictionary<string, GenericElement> properties = [];
            foreach (var keyValue in Properties)
            {
                properties.Add(keyValue.Key, keyValue.Value.Clone());
            }
            return new GenericObject(properties) { Parent = Parent };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentIdentifiers"></param>
        /// <returns></returns>
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
                else
                {
                    references.Add(new GenericReference(identifier, property.Value));
                }
            }
            return references;
        }

        public override void ApplyContext(Dictionary<string, GenericReference> contextReferences, int depth = 0)
        {
            foreach (var property in Properties)
            {
                property.Value.ApplyContext(contextReferences, depth);
            }
        }

        public bool DoesPropertyExist(string name)
        {
            return Properties.ContainsKey(name);
        }

        public void RenameProperty(string oldName, string newName)
        {
            if (Properties.ContainsKey(newName))
                return;

            Properties[newName] = Properties[oldName];
            Properties.Remove(oldName);
        }

        public void Remove(string property)
        {
            Properties.Remove(property);
            NotifypropertyChanged(nameof(ValuesProperties));
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
    }
}