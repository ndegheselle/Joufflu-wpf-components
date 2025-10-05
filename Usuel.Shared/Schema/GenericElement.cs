using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Input;

namespace Usuel.Shared.Schema
{
    public interface IGenericElement : ICloneable
    {
        IGenericParent? Parent { get; set; }
        IEnumerable<IGenericParent> ParentsTree { get; }
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
