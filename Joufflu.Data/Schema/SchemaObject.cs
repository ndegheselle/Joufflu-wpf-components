using System.Collections;
using System.Collections.ObjectModel;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json.Serialization;
using Usuel.Shared;

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
    {
        IGenericNode ToValue();
    }

    public class SchemaValue : ISchemaElement
    {
        public EnumDataType DataType { get; set; } = EnumDataType.String;
        public IGenericNode ToValue() => new GenericValue(this);
    }

    public interface ISchemaParent : ISchemaElement
    {
        public IEnumerable Childrens { get; }
    }

    public class SchemaArray : ISchemaParent
    {
        public IEnumerable Childrens => new List<ISchemaElement>() { Type };
        /// <summary>
        /// Contain the type of the array
        /// </summary>
        public ISchemaElement Type { get; set; }
        [JsonIgnore]
        public ISchemaParent? Parent { get; set; }

        public ICustomCommand UseValueCommand { get; set; }
        public ICustomCommand UseArrayCommand { get; set; }
        public ICustomCommand UseObjectCommand { get; set; }

        public SchemaArray()
        {
            Type = new SchemaValue();
            UseValueCommand = new DelegateCommand(() => Type = new SchemaValue());
            UseArrayCommand = new DelegateCommand(() => Type = new SchemaArray());
            UseObjectCommand = new DelegateCommand(() => Type = new SchemaObject());
        }

        public IGenericNode ToValue() => new GenericArray(this);
    }

    public class SchemaProperty
    {
        public string Name { get; set; }
        public ISchemaElement Element { get; }

        [JsonIgnore]
        public SchemaObject Parent { get; }
        public ICustomCommand RemoveCommand { get; }

        public SchemaProperty(SchemaObject parent, string name, ISchemaElement element)
        {
            Name = name;
            Parent = parent;
            Element = element;
            RemoveCommand = new DelegateCommand(() => Parent.Remove(this));
        }
    }

    public class SchemaParentProperty : SchemaProperty
    {
        public ISchemaParent ElementParent => (ISchemaParent)Element;
        public SchemaParentProperty(SchemaObject parent, string name, ISchemaParent element) : base(parent, name, element)
        {}
    }

    public class SchemaObject : ISchemaParent
    {
        [JsonIgnore]
        public ISchemaParent? Parent { get; set; }

        public IEnumerable Childrens => Properties;
        public ObservableCollection<SchemaProperty> Properties { get; } = [];

        public ICustomCommand AddValueCommand { get; }
        public ICustomCommand AddArrayCommand { get; }
        public ICustomCommand AddObjectCommand { get; }

        public SchemaObject()
        {
            AddValueCommand = new DelegateCommand(() => Add("Default", new SchemaValue()));
            AddArrayCommand = new DelegateCommand(() => Add("Default", new SchemaArray()));
            AddObjectCommand = new DelegateCommand(() => Add("Default", new SchemaObject()));
        }

        public SchemaObject Add(string name, ISchemaElement element)
        {
            if (element is ISchemaParent parent)
                Properties.Add(new SchemaParentProperty(this, GetUniquePropertyName(name), parent));
            else
                Properties.Add(new SchemaProperty(this, GetUniquePropertyName(name), element));
            return this;
        }

        public void Remove(SchemaProperty property) { Properties.Remove(property); }

        public IGenericNode ToValue()
        {
            Dictionary<string, IGenericNode> values = Properties.ToDictionary(
                prop => prop.Name ?? "",
                prop => prop.Element.ToValue());

            return new GenericObject(this, values);
        }

        private bool IsPropertyNameUnique(string name)
        {
            return Properties.Any(p => (p.Name ?? "").Trim()
                    .ToLowerInvariant()
                    .Equals(name.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase)
                    ) == false;
        }

        /// <summary>
        /// Gets a unique property name by appending a numeric extension if the name already exists.
        /// </summary>
        /// <param name="baseName">The base name to make unique</param>
        /// <returns>A unique property name</returns>
        private string GetUniquePropertyName(string baseName)
        {
            if (IsPropertyNameUnique(baseName))
                return baseName;

            int counter = 1;
            string uniqueName;
            do
            {
                uniqueName = $"{baseName} {counter}";
                counter++;
            } while (IsPropertyNameUnique(uniqueName) == false);

            return uniqueName;
        }
    }
}
