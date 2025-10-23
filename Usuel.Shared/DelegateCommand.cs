using System.Windows.Input;

namespace Usuel.Shared
{
    public interface ICustomCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }

    public class DelegateCommand : ICustomCommand
    {
        private readonly Action _action;
        private readonly Func<bool>? _condition;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand(Action action, Func<bool>? executeCondition = default)
        {
            _action = action;
            _condition = executeCondition;
        }

        public bool CanExecute(object? parameter) => _condition?.Invoke() ?? true;
        public virtual void Execute(object? parameter) => _action();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }

    public class DelegateCommand<T> : ICustomCommand
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool>? _condition;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand(Action<T> action, Func<T, bool>? executeCondition = null)
        {
            _action = action;
            _condition = executeCondition;
        }

        public bool CanExecute(object? parameter) => CanExecute(parameter is T value ? value : default!);
        public bool CanExecute(T parameter) => _condition?.Invoke(parameter) ?? true;

        public virtual void Execute(object? parameter) => Execute(parameter is T value ? value : default!);
        public void Execute(T parameter) => _action(parameter);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
