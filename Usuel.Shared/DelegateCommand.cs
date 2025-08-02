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

        public bool CanExecute(object? parameter)
        {
            if (parameter is T value)
            {
                return _condition?.Invoke(value) ?? true;
            }
            return _condition?.Invoke(default!) ?? true;
        }

        public virtual void Execute(object? parameter)
        {
            if (parameter is T value)
            {
                _action(value);
            }
            else
            {
                _action(default!);
            }
        }

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());
    }
}
