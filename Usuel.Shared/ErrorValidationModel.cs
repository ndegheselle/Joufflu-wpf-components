using System.Collections;
using System.ComponentModel;

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

        private void OnErrorsChanged(string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public void AddError(string propertyName, string error)
        {
            AddErrors(propertyName, new List<string> { error });
        }

        public void AddErrors(string propertyName, List<string> errors)
        {
            if (!_errorsByPropertyName.ContainsKey(propertyName))
                _errorsByPropertyName[propertyName] = new List<string>();

            _errorsByPropertyName[propertyName].AddRange(errors);
            OnErrorsChanged(propertyName);
        }

        public void AddErrors(Dictionary<string, List<string>> errors)
        {
            foreach (var error in errors)
            {
                AddErrors(error.Key, error.Value);
            }
        }

        public void ClearErrors(string? propertyName = null)
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
