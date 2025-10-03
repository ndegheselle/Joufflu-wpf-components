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
