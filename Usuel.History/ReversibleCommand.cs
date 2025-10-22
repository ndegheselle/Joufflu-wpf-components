using Usuel.Shared;

namespace Usuel.History
{
    public interface IReversibleCommand : ICustomCommand
    {
        public string Name { get; }
        /// <summary>
        /// Command that does the exact oposit of this command.
        /// </summary>
        public IReversibleCommand? Reverse { get; set; }
        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="withHistory">If the command should add an history line.</param>
        public void Execute(object? parameter = null, bool withHistory = true);
    }

    public class ReversibleCommand : DelegateCommand, IReversibleCommand
    {
        public string Name { get; }

        public IReversibleCommand? Reverse { get; set; }

        private readonly HistoryHandler _handler;

        public ReversibleCommand(
            HistoryHandler handler,
            Action action,
            Func<bool>? executeCondition = default,
            string name = "") : base(action, executeCondition)
        {
            _handler = handler;
            Name = name;
        }

        public override void Execute(object? parameter = null) => Execute(parameter, true);

        public void Execute(object? parameter = null, bool withHistory = true)
        {
            base.Execute(parameter);
            if (withHistory)
                _handler.Add(this, parameter);
        }
    }

    public class ReversibleCommand<T> : DelegateCommand<T>, IReversibleCommand
    {
        public string Name { get; }

        public IReversibleCommand? Reverse { get; set; }

        private readonly HistoryHandler _handler;

        public ReversibleCommand(HistoryHandler handler, Action<T> action, Func<T, bool>? executeCondition = default, string name = "") : base(
            action,
            executeCondition)
        { 
            _handler = handler;
            Name = name;
        }

        public override void Execute(object? parameter = null) => Execute(parameter, true);

        public void Execute(object? parameter = null, bool withHistory = true)
        {
            base.Execute(parameter);
            if (withHistory)
                _handler.Add(this, parameter);
        }
    }
}
