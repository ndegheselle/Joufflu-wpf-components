using System;
using System.Windows.Input;

namespace WpfComponents.Lib.Logic
{
    public class SimpleCommand<T> : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action<T> _Execute;
        private readonly Predicate<T>? _CanExecute;

        public SimpleCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        public SimpleCommand(Action<T> execute, Predicate<T>? canExecute)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _CanExecute?.Invoke((T)parameter) ?? true;
        }

        public void Execute(object? parameter)
        {
            _Execute?.Invoke((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

    }

    public class SimpleCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        private readonly Action _Execute;
        private readonly Func<bool>? _CanExecute;

        public SimpleCommand(Action execute)
            : this(execute, null)
        {
        }

        public SimpleCommand(Action execute, Func<bool>? canExecute)
        {
            _Execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _CanExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _CanExecute?.Invoke() ?? true;
        }

        public void Execute(object? parameter)
        {
            _Execute();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

    }
}
