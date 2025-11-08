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
            Add(propertyName, [error]);
        }

        public void Add(string? propertyName, List<string> errors)
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
                Add(error.Key, error.Value);
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
}
