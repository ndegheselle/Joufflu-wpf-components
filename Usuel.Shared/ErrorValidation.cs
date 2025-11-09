using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Usuel.Shared
{
    public class ErrorValidation : INotifyDataErrorInfo
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        
        public bool HasErrors => _errors.Count > 0;

        private readonly Dictionary<string, List<string>> _errors = [];
        
        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null)
                return _errors.Values;
            return _errors.TryGetValue(propertyName, out var value) ? value : [];
        }

        private void NotifyErrorsChanged(string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void Add(string error, [CallerMemberName] string? propertyName = null)
        {
            Add([error], propertyName);
        }

        public void Add(IEnumerable<string> errors, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = [];

            _errors[propertyName].AddRange(errors);
            NotifyErrorsChanged(propertyName);
        }

        public void Add(Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                Add(error.Value, error.Key);
            }
        }

        public void Clear([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
            {
                foreach (var value in _errors)
                    Clear(value.Key);
                return;
            }

            if (_errors.Remove(propertyName))
            {
                NotifyErrorsChanged(propertyName);
            }
        }
    }

    public class BaseErrorModel : INotifyDataErrorInfo
    {
        public ErrorValidation Errors { get; } = new ErrorValidation();
        public IEnumerable GetErrors(string? propertyName) => Errors.GetErrors(propertyName);

        public bool HasErrors => Errors.HasErrors;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged
        {
            add => Errors.ErrorsChanged += value;
            remove => Errors.ErrorsChanged -= value;
        }
    }
}
