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
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _condition = executeCondition;
        }

        public bool CanExecute(object? parameter) => _condition?.Invoke() ?? true;

        public void Execute(object? parameter) => _action();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }

    public class DelegateCommand<T> : ICustomCommand
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool>? _condition;

        public event EventHandler? CanExecuteChanged;

        public DelegateCommand(Action<T> action, Func<T, bool>? executeCondition = default)
        {
            _action = action ?? throw new ArgumentNullException(nameof(action));
            _condition = executeCondition;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is T value)
            {
                return _condition?.Invoke(value) ?? true;
            }
            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T value)
            {
                _action(value);
            }
            else
            {
                throw new ArgumentException($"Type '{parameter?.GetType()}' of parameter is not assignable to type '{typeof(T)}'.");
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
