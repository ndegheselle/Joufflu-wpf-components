using System.ComponentModel;
using System.Runtime.CompilerServices;
using Usuel.Shared;

namespace Usuel.History
{
    public interface IHistoryAction
    {
        public bool CanReverse { get; }
        public IHistoryAction ExecuteReverse();
    }

    public class HistoryAction : IHistoryAction
    {
        public IReversibleCommand Command { get; }
        public object? Parameter { get; }

        public bool CanReverse => Command.Reverse != null;

        public HistoryAction(IReversibleCommand command, object? parameter = null)
        {
            Command = command;
            Parameter = parameter;
        }

        public IHistoryAction ExecuteReverse()
        {
            if (Command.Reverse == null)
                throw new Exception("This action can't be reversed.");

            // History line is added by the handler in the correct stack (undo or redo)
            Command.Reverse.Execute(Parameter, withHistory: false);
            return new HistoryAction(Command.Reverse, Parameter);
        }

        public override string ToString() => Command.Name;
    }

    public class HistoryHandler : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string? name = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }

        private bool _isUndoAvailable = false;
        public bool IsUndoAvailable
        {
            get => _isUndoAvailable;
            private set
            {
                if (_isUndoAvailable == value)
                    return;

                _isUndoAvailable = value;
                NotifyPropertyChanged();
                UndoCommand.RaiseCanExecuteChanged();
            }
        }
        private bool _isRedoAvailable = false;
        public bool IsRedoAvailable
        {
            get => _isRedoAvailable;
            private set
            {
                if (_isRedoAvailable == value)
                    return;

                _isRedoAvailable = value;
                NotifyPropertyChanged();
                RedoCommand.RaiseCanExecuteChanged();
            }
        }

        public ICustomCommand UndoCommand { get; }
        public ICustomCommand RedoCommand { get; }

        public List<IHistoryAction> Stack => _undoStack.ToList();

        private Stack<IHistoryAction> _undoStack = [];
        private Stack<IHistoryAction> _redoStack = [];

        public HistoryHandler()
        {
            UndoCommand = new DelegateCommand(Undo, () => IsUndoAvailable);
            RedoCommand = new DelegateCommand(Redo, () => IsRedoAvailable);
        }

        public void SetReverse(IReversibleCommand command1, IReversibleCommand command2)
        {
            command1.Reverse = command2;
            command2.Reverse = command1;
        }

        public void Add(IReversibleCommand command, object? parameter = null)
        {
            Add(new HistoryAction(command, parameter));
        }

        public void Add(IHistoryAction action)
        {
            _undoStack.Push(action);
            _redoStack.Clear();

            IsRedoAvailable = _redoStack.Count!=0;
            IsUndoAvailable = _undoStack.Count!=0;
            NotifyPropertyChanged(nameof(Stack));
        }

        public void Undo()
        {
            if (IsUndoAvailable == false)
                return;

            var action = _undoStack.Pop();
            IsUndoAvailable = _undoStack.Count!=0;
            NotifyPropertyChanged(nameof(Stack));

            // Ignore the command that don't have any undo command
            if (action.CanReverse == false)
                return;
            var reverseAction = action.ExecuteReverse();
            _redoStack.Push(reverseAction);
            IsRedoAvailable = _redoStack.Count!=0;
        }

        public void Redo()
        {
            if (IsRedoAvailable == false)
                return;

            var action = _redoStack.Pop();
            IsRedoAvailable = _redoStack.Count!=0;

            // Ignore the command that don't have any undo command
            if (action.CanReverse == false)
                return;
            action.ExecuteReverse();
            _undoStack.Push(action);
            IsUndoAvailable = _undoStack.Count!=0;
            NotifyPropertyChanged(nameof(Stack));
        }
    }
}
