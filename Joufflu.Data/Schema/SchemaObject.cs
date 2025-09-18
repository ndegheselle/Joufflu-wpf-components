using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Usuel.Shared;

namespace Joufflu.Data.Schema
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

    public interface ISchemaElement
    {
        ISchemaParent? Parent { get; set; }
        IEnumerable<ISchemaParent> ParentsTree { get; }
        IGenericNode ToValue();
    }

    public class SchemaValue : ISchemaElement
    {
        [JsonIgnore]
        public ISchemaParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<ISchemaParent> ParentsTree => Parent?.Parent == null ? [] : [..Parent.ParentsTree, Parent];
        public EnumDataType DataType { get; set; } = EnumDataType.String;
        public IGenericNode ToValue() => new GenericValue(this);
    }

    public interface ISchemaParent : ISchemaElement
    {
        IList<SchemaProperty> Childrens { get; }
        public void Remove(SchemaProperty property)
        {
            Childrens.Remove(property);
        }
    }

    public class SchemaArray : ISchemaParent, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public IList<SchemaProperty> Childrens => new List<SchemaProperty>() { Type };

        /// <summary>
        /// Contain the type of the array
        /// </summary>
        private SchemaProperty _type;
        public SchemaProperty Type
        {
            get => _type;
            set
            {
                if (value == _type)
                    return;
                _type = value;
                NotifyPropertyChanged(nameof(Childrens));
            }
        }

        [JsonIgnore]
        public ISchemaParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<ISchemaParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public ICustomCommand UseValueCommand { get; set; }
        public ICustomCommand UseArrayCommand { get; set; }
        public ICustomCommand UseObjectCommand { get; set; }

        public SchemaArray()
        {
            _type = new SchemaProperty("Type", new SchemaValue() { Parent = this }) { IsConst = true };
            UseValueCommand = new DelegateCommand(() => Type = new SchemaProperty("Type", new SchemaValue() { Parent = this }) { IsConst = true });
            UseArrayCommand = new DelegateCommand(() => Type = new SchemaParentProperty("Type", new SchemaArray() { Parent = this }) { IsConst = true });
            UseObjectCommand = new DelegateCommand(() => Type = new SchemaParentProperty("Type", new SchemaObject() { Parent = this }) { IsConst = true });
        }

        public IGenericNode ToValue() => new GenericArray(this);
    }

    public class SchemaProperty
    {
        public string Name { get; set; }
        public ISchemaElement Element { get; }
        public ICustomCommand RemoveCommand { get; }
        public bool IsConst { get; set; } = false;

        public SchemaProperty(string name, ISchemaElement element)
        {
            Name = name;
            Element = element;
            RemoveCommand = new DelegateCommand(() => Element.Parent?.Remove(this), () => IsConst == false);
        }
    }

    public class SchemaParentProperty : SchemaProperty
    {
        public ISchemaParent ElementParent => (ISchemaParent)Element;
        public SchemaParentProperty(string name, ISchemaParent element) : base(name, element)
        { }
    }

    public class SchemaObject : ISchemaParent
    {
        [JsonIgnore]
        public ISchemaParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<ISchemaParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public IList<SchemaProperty> Childrens => Properties;
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
            element.Parent = this;
            if (element is ISchemaParent parent)
                Properties.Add(new SchemaParentProperty(GetUniquePropertyName(name), parent));
            else
                Properties.Add(new SchemaProperty(GetUniquePropertyName(name), element));
            return this;
        }

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
