using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Usuel.Shared;

namespace Joufflu.Proto.Data
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

    /// <summary>
    /// Eelement with childrens.
    /// </summary>
    public interface IGenericParent : IGenericElement, INotifyPropertyChanged
    { }

    public interface IGenericElement : ICloneable
    {
        IGenericParent? Parent { get; set; }
        IEnumerable<IGenericParent> ParentsTree { get; }
    }

    /// <summary>
    /// Reference to another element (identifier can be nested with dot separator).
    /// XXX : very similar to GenericPropery, maybe fuse them ?
    /// </summary>
    public class GenericReference
    {
        public string Identifier { get; }
        public EnumDataType DataType { get; }
        public GenericElement Element { get; }

        public GenericReference(string identifier, GenericElement element)
        {
            Identifier = identifier;
            Element = element;
            DataType = element switch
            {
                _ when element is GenericObject => EnumDataType.Object,
                _ when element is GenericArray => EnumDataType.Array,
                _ when element is GenericValue value => value.DataType,
                _ => throw new NotImplementedException($"The element type '{element.GetType()}' is not handled.")
            };
        }

        public override string ToString() => Identifier;

        public override bool Equals(object? obj)
        {
            return obj is GenericReference other && string.Equals(Identifier, other.Identifier);
        }
        public override int GetHashCode() => Identifier.GetHashCode();
    }

    public abstract class GenericElement : IGenericElement, ICloneable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        [JsonIgnore]
        public IGenericParent? Parent { get; set; }
        [JsonIgnore]
        public IEnumerable<IGenericParent> ParentsTree => Parent?.Parent == null ? [] : [.. Parent.ParentsTree, Parent];

        public string ContextReference { get; set; } = string.Empty;

        public ICommand ClearReferenceCommand { get; private set; }

        public bool IsExpanded { get; set; }

        public GenericElement()
        {
            ClearReferenceCommand = new DelegateCommand<bool>((clear) =>
            {
                if (clear)
                    ContextReference = string.Empty;
            });
        }

        /// <summary>
        /// Apply a context to a generic element and all its childrens, will change all the values by the referenced context element value.
        /// </summary>
        /// <param name="context"></param>
        public void ApplyContext(GenericObject context)
        {
            Dictionary<string, GenericReference> contextReferences = context.GetReferences().ToDictionary(x => x.Identifier, x => x);
            ApplyContext(contextReferences);
        }
        public abstract void ApplyContext(Dictionary<string, GenericReference> contextReferences, int depth = 0);

        public abstract GenericElement Clone();
        object ICloneable.Clone()
        {
            return Clone();
        }

        protected void NotifypropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}