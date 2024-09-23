using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Usuel.Shared
{
    public class ErrorValidationModel : INotifyDataErrorInfo
    {
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => _errorsByPropertyName.Any();
        private readonly Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();

        public IEnumerable GetErrors(string? propertyName)
        {
            if (propertyName == null)
                return _errorsByPropertyName.Values;
            return _errorsByPropertyName.ContainsKey(propertyName) ? _errorsByPropertyName[propertyName] : [];
        }

        private void OnErrorsChanged([CallerMemberName]string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void AddError(string error, [CallerMemberName] string? propertyName = null)
        {
            AddErrors(new List<string> { error }, propertyName);
        }

        public void AddErrors(List<string> errors, [CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
                return;

            if (!_errorsByPropertyName.ContainsKey(propertyName))
                _errorsByPropertyName[propertyName] = new List<string>();

            _errorsByPropertyName[propertyName].AddRange(errors);
            OnErrorsChanged(propertyName);
        }

        public void AddErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                AddErrors(error.Value, error.Key);
            }
        }

        public void ClearErrors([CallerMemberName] string? propertyName = null)
        {
            if (propertyName == null)
            {
                _errorsByPropertyName.Clear();
                OnErrorsChanged();
                return;
            }

            if (_errorsByPropertyName.ContainsKey(propertyName))
            {
                _errorsByPropertyName.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }
        }
    }
}
