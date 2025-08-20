using Joufflu.Shared.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Xml.Linq;
using Usuel.Shared;

namespace Joufflu.Data.Schema
{
    public enum EnumValueType
    {
        Object,
        String,
        Decimal,
        Boolean,
        DateTime,
        TimeSpan
    }

    public class SchemaProperty : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        public ICustomCommand RemoveCommand { get; set; }

        public string Name { get; set; } = string.Empty;

        private EnumValueType _type = EnumValueType.String;

        public EnumValueType Type
        {
            get => _type;
            set
            {
                if (_type == value)
                    return;
                _type = value;
                // If we have a parent, notify it to update its collection
                Parent?.UpdatePropertyType(this, value);
            }
        }

        public SchemaObject? Parent { get; set; } = null;
        public uint Depth { get; set; } = 0;
        public bool IsHovered { get; set; }

        public SchemaProperty(string name, EnumValueType type)
        {
            RemoveCommand = new DelegateCommand(Remove);
            Name = name;
            Type = type;
        }

        /// <summary>
        /// Removes this property from its parent schema object.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void Remove()
        {
            if (Parent == null)
                throw new Exception("Can't remove a property that doesn't have a parent.");

            Parent.Remove(this);
        }

        public override string ToString() => Name;
    }

    public class SchemaValue : SchemaProperty
    {
        public dynamic? Value { get; set; }

        public SchemaValue(string name, EnumValueType type, dynamic? value = null) : base(name, type)
        {
            Value = value;
        }
    }

    public class SchemaObject : SchemaProperty
    {
        public ObservableCollection<SchemaProperty> Properties { get; private set; } = [];

        public ICustomCommand AddPropertyCommand { get; set; }

        public SchemaObject(string name) : base(name, EnumValueType.Object) 
        {
            AddPropertyCommand = new DelegateCommand(() => AddValue("default"));
        }

        /// <summary>
        /// Adds a property to the schema object.
        /// </summary>
        /// <param name="property">Property</param>
        /// <param name="index">Index of the property to add</param>
        /// <returns>This</returns>
        public SchemaObject Add(SchemaProperty property, int index = -1)
        {
            property.Name = GetUniquePropertyName(property.Name);
            property.Depth = Depth + 1;
            property.Parent = this;
            if (index >= 0 && index < Properties.Count)
            {
                Properties.Insert(index, property);
            }
            else if (index == -1 || index >= Properties.Count)
            {
                Properties.Add(property);
            }
            return this;
        }

        /// <summary>
        /// Adds a new object to the schema with the specified name.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <returns>This</returns>
        public SchemaObject AddObject(string name)
        {
            return Add(new SchemaObject(name));
        }

        /// <summary>
        /// Adds a new value to the schema with the specified name and type.
        /// </summary>
        /// <param name="name">Property name</param>
        /// <param name="type">Type of the value</param>
        /// <param name="value">Value</param>
        /// <returns>This</returns>
        public SchemaObject AddValue(string name, EnumValueType type = EnumValueType.String, dynamic? value = null)
        {
            return Add(new SchemaValue(name, type, value));
        }

        /// <summary>
        /// Updates the type of a property in the schema object.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="newType"></param>
        public void UpdatePropertyType(SchemaProperty property, EnumValueType newType)
        {
            int index = Properties.IndexOf(property);
            if (index == -1) return;

            // Remove old property
            Properties.RemoveAt(index);

            // Create new property of correct type at same position
            SchemaProperty newProperty = newType == EnumValueType.Object
                ? new SchemaObject(property.Name)
                : new SchemaValue(property.Name, newType);
            Add(newProperty, index);
        }

        /// <summary>
        /// Removes a property from the schema object.
        /// </summary>
        /// <param name="property"></param>
        /// <exception cref="Exception"></exception>
        public void Remove(SchemaProperty property)
        {
            if (Properties.Remove(property))
            {
                property.Parent = null; // Clear parent reference
            }
            else
            {
                throw new Exception("Property not found in the collection.");
            }
        }

        /// <summary>
        /// Gets a unique property name by appending a numeric extension if the name already exists.
        /// </summary>
        /// <param name="baseName">The base name to make unique</param>
        /// <returns>A unique property name</returns>
        private string GetUniquePropertyName(string baseName)
        {
            if (!Properties.Any(p => p.Name.Equals(baseName, StringComparison.OrdinalIgnoreCase)))
            {
                return baseName;
            }

            int counter = 1;
            string uniqueName;
            do
            {
                uniqueName = $"{baseName} {counter}";
                counter++;
            }
            while (Properties.Any(p => p.Name.Equals(uniqueName, StringComparison.OrdinalIgnoreCase)));

            return uniqueName;
        }
    }
}