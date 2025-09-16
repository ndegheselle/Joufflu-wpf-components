using System.Collections.ObjectModel;
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

    public interface ISubSchemaElement : ISchemaElement
    {
        string? Name { get; set; }
        SchemaObject? Parent { get; set; }
        bool IsSelected { get; }

        ICustomCommand RemoveCommand { get; }
    }

    public class SchemaValue : ISubSchemaElement
    {
        public string? Name { get; set; }
        public EnumDataType DataType { get; set; } = EnumDataType.String;

        [JsonIgnore]
        public SchemaObject? Parent { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public ICustomCommand RemoveCommand { get; set; }

        public SchemaValue() {
            RemoveCommand = new DelegateCommand(() => Parent?.Remove(this)); 
        }

        public IGenericNode ToValue() { 
            return new GenericValue(this); 
        }
    }

    public interface ISchemaParent : ISubSchemaElement
    {
        public ICustomCommand AddValueCommand { get; }
        public ICustomCommand AddArrayCommand { get; }
        public ICustomCommand AddObjectCommand { get; }
        void Remove(ISubSchemaElement property);
    }

    public class SchemaArray : ISchemaParent
    {
        public string? Name { get; set; }
        /// <summary>
        /// Contain the type of the array
        /// </summary>
        public ISubSchemaElement Type { get; set; }

        [JsonIgnore]
        public SchemaObject? Parent { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public ICustomCommand AddValueCommand { get; set; }
        public ICustomCommand AddArrayCommand { get; set; }
        public ICustomCommand AddObjectCommand { get; set; }
        public ICustomCommand RemoveCommand { get; set; }

        public SchemaArray()
        {
            Type = new SchemaValue() {Parent=this, Name = "Type"};
            UseValueCommand = new DelegateCommand(() => Type = new SchemaValue());
            UseArrayCommand = new DelegateCommand(() => Type = new SchemaArray());
            UseObjectCommand = new DelegateCommand(() => Type = new SchemaObject());
            RemoveCommand = new DelegateCommand(() => Parent?.Remove(this));
        }

        public IGenericNode ToValue()
        {
            return new GenericArray(this);
        }
    }

    public class SchemaObject : ISchemaParent
    {
        public string? Name { get; set; }

        [JsonIgnore]
        public SchemaObject? Parent { get; set; }
        [JsonIgnore]
        public bool IsSelected { get; set; }

        public ObservableCollection<ISubSchemaElement> Properties { get; } = [];

        public ICustomCommand AddValueCommand { get; }
        public ICustomCommand AddArrayCommand { get; }
        public ICustomCommand AddObjectCommand { get; }
        public ICustomCommand RemoveCommand { get; }

        public SchemaObject()
        {
            AddValueCommand = new DelegateCommand(() => Parent?.Add("Default", new SchemaValue()));
            AddArrayCommand = new DelegateCommand(() => Parent?.Add("Default", new SchemaArray()));
            AddObjectCommand = new DelegateCommand(() => Parent?.Add("Default", new SchemaObject()));
            RemoveCommand = new DelegateCommand(() => Parent?.Remove(this));
        }

        public SchemaObject Add(string name, ISubSchemaElement element)
        {
            element.Parent = this;
            element.Name = GetUniquePropertyName(name);
            Properties.Add(element);
            return this;
        }

        public void Remove(ISubSchemaElement property) { Properties.Remove(property); }

        public IGenericNode ToValue()
        {
            Dictionary<string, IGenericNode> values = Properties.ToDictionary(
                prop => prop.Name ?? "",
                prop => prop.ToValue());

            return new GenericObject(this, values);
        }

        private bool IsPropertyNameUnique(string name)
        {
            return !Properties.Any(
                p => (p.Name ?? "").Trim()
                    .ToLowerInvariant()
                    .Equals(name.Trim().ToLowerInvariant(), StringComparison.OrdinalIgnoreCase));
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
