using System.Text.Json.Serialization;
using Usuel.Shared;

namespace Joufflu.Data.Shared
{
     /// <summary>
     /// Array of <see cref="GenericElement"/>.
     /// </summary>
    public partial class GenericArray : GenericElement, IGenericParent
    {
        // XXX : Having a GenericProperty in a GenericArray can cause problem if the user try to remove the element
        [JsonIgnore]
        public IEnumerable<GenericProperty> MetadataProperties => [new GenericProperty("Schema", Schema) { IsRemovable = false, IsIdentifierEditable = false }];
        public ICustomCommand ChangeSchemaCommand { get; }

        /// <summary>
        /// Schema that represent the kind of data that is stored in the values.
        /// </summary>
        public GenericElement Schema { get; private set; }

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
            IsExpanded = true;
            NotifypropertyChanged(nameof(MetadataProperties));
        }
    }


    /// <summary>
    /// Object composed of properties of <see cref="GenericElement"/>.
    /// </summary>
    public partial class GenericObject : GenericElement, IGenericParent
    {
        public ICustomCommand CreatePropertyCommand { get; }

        public GenericObject(Dictionary<string, GenericElement>? properties = null)
        {
            Properties = properties ?? [];
            CreatePropertyCommand = new DelegateCommand<EnumDataType>((type) => CreateProperty("Default", type));

            if (properties == null)
                CreateProperty("Default", EnumDataType.String);
        }

        #region Properties
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
            IsExpanded = true;
            AddProperty(GetUniqueName(name), element);
        }

        /// <summary>
        /// Add a property with a specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="element"></param>
        public void AddProperty(string name, GenericElement element)
        {
            element.Parent = this;
            Properties.Add(name, element);
            NotifypropertyChanged(nameof(WrappedProperties));
        }

        /// <summary>
        /// Remove a property by name.
        /// </summary>
        /// <param name="property"></param>
        public void Remove(string property)
        {
            Properties.Remove(property);
            NotifypropertyChanged(nameof(WrappedProperties));
        }

        /// <summary>
        /// Check if a property with a specific name exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool DoesPropertyExist(string name)
        {
            return Properties.ContainsKey(name);
        }

        /// <summary>
        /// Rename a property.
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="newName"></param>
        public void RenameProperty(string oldName, string newName)
        {
            if (Properties.ContainsKey(newName))
                return;

            Properties[newName] = Properties[oldName];
            Properties.Remove(oldName);
        }

        /// <summary>
        /// Gets a unique property name by appending a numeric extension if the name already exists.
        /// </summary>
        /// <param name="baseName">The base name to make unique</param>
        /// <returns>A unique property name</returns>
        private string GetUniqueName(string baseName)
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
        #endregion
    }
}