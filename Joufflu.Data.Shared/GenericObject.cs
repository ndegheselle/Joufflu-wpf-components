using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Usuel.Shared;

namespace Joufflu.Data.Shared
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
            if (Parent.DoesPropertyExist(newName) == true)
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
    public partial class GenericArray : GenericElement, IGenericParent
    {
        [JsonIgnore]
        public IEnumerable<GenericIndex> IndexedValues => Values.Select((val, index) => new GenericIndex(index, val));

        public ICustomCommand AddValueCommand { get; }
        public IList<GenericElement> Values { get; }

        #region Values
        /// <summary>
        /// Create a value based on the schema.
        /// </summary>
        public void CreateValue()
        {
            Values.Add(Schema.Clone());
            IsExpanded = true;
            NotifypropertyChanged(nameof(IndexedValues));
        }

        /// <summary>
        /// Remove the item at the specific index.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            Values.RemoveAt(index);
            NotifypropertyChanged(nameof(IndexedValues));
        }
        #endregion

        #region Context

        public override void ApplyContext(Dictionary<string, GenericReference> contextReferences, int depth = 0)
        {
            foreach (var value in Values)
            {
                value.ApplyContext(contextReferences, depth);
            }
        }

        #endregion

        public override GenericElement Clone() => new GenericArray(Schema, Values.Select(x => x.Clone()).ToList()) { Parent = Parent };
    }


    /// <summary>
    /// Object composed of properties of <see cref="GenericElement"/>.
    /// </summary>
    public partial class GenericObject : GenericElement, IGenericParent
    {
        [JsonIgnore]
        public IEnumerable<GenericProperty> WrappedProperties => Properties.Select(x => new GenericProperty(x.Key, x.Value));
        public Dictionary<string, GenericElement> Properties { get; }

        #region Context
        /// <summary>
        /// Get all references of the properties and nested obects
        /// </summary>
        /// <param name="parentIdentifiers"></param>
        /// <returns></returns>
        public IEnumerable<GenericReference> GetReferences(IEnumerable<string>? parentIdentifiers = null)
        {
            parentIdentifiers = parentIdentifiers ?? [];
            List<GenericReference> references = new List<GenericReference>();
            foreach (var property in Properties)
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

        /// <summary>
        /// Apply context to all properties.
        /// </summary>
        /// <param name="contextReferences"></param>
        /// <param name="depth"></param>
        public override void ApplyContext(Dictionary<string, GenericReference> contextReferences, int depth = 0)
        {
            foreach (var property in Properties)
            {
                property.Value.ApplyContext(contextReferences, depth);
            }
        }
        #endregion

        public override GenericElement Clone()
        {
            Dictionary<string, GenericElement> properties = [];
            foreach (var keyValue in Properties)
            {
                properties.Add(keyValue.Key, keyValue.Value.Clone());
            }
            return new GenericObject(properties) { Parent = Parent };
        }
    }
}