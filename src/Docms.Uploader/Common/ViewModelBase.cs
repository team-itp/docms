using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Docms.Uploader.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void SetProperty<T>(ref T member, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(member, value))
            {
                return;
            }
            member = value;
            OnPropertyChanged(propertyName);
        }
    }

    public abstract class ValidationViewModelBase : ViewModelBase, IDataErrorInfo, INotifyDataErrorInfo
    {
        private ValidationContext context;
        private bool _startValidation;

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public ValidationViewModelBase()
        {
            Reset();
        }

        string IDataErrorInfo.this[string propertyName] => GetErrors(propertyName)?.Cast<string>().FirstOrDefault();
        string IDataErrorInfo.Error => GetErrors("")?.Cast<string>().FirstOrDefault();
        public bool HasErrors => !string.IsNullOrEmpty(((IDataErrorInfo)this).Error);

        public void Reset()
        {
            context = new ValidationContext(this);
            _startValidation = false;
        }

        public void Validate()
        {
            _startValidation = true;
            foreach (var prop in GetType().GetProperties())
            {
                OnErrosChanged(prop.Name);
            }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (!_startValidation)
            {
                return null;
            }

            var results = new List<ValidationResult>();
            if (string.IsNullOrEmpty(propertyName))
            {
                if (Validator.TryValidateObject(this, context, results))
                {
                    return null;
                }
                return results.Select(e => e.ErrorMessage);
            }
            else
            {
                if (Validator.TryValidateProperty(GetValue(propertyName), new ValidationContext(this, null, null) { MemberName = propertyName }, results))
                {
                    return null;
                }
                return results.Select(e => e.ErrorMessage);
            }
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            _startValidation = true;
            base.OnPropertyChanged(propertyName);
            OnErrosChanged(propertyName);
        }

        protected virtual void OnErrosChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private object GetValue(string propertyName)
        {
            return GetType().GetProperty(propertyName).GetValue(this);
        }
    }
}
