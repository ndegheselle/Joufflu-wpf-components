using System.Collections.ObjectModel;

namespace Usuel.Shared.Data
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

    public class SchemaProperty
    {
        public SchemaObject? Parent { get; set; } = null;
        public uint Depth { get; set; } = 0;
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

        public SchemaProperty(string name, EnumValueType type)
        {
            Name = name;
            Type = type;
        }
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
        public ObservableCollection<SchemaProperty> Properties { get; set; } = [];

        public ICustomCommand AddProperty { get; set; }

        public SchemaObject(string name) : base(name, EnumValueType.Object) 
        {
            AddProperty = new DelegateCommand(() => AddValue("default"));
        }

        public SchemaObject Add(SchemaProperty property, int index = -1)
        {
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

        public SchemaObject AddObject(string name)
        {
            return Add(new SchemaObject(name));
        }

        public SchemaObject AddValue(string name, EnumValueType type = EnumValueType.String, dynamic? value = null)
        {
            return Add(new SchemaValue(name, type, value));
        }

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

    }
}
