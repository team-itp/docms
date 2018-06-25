using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Docms.Uploader.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
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

        string IDataErrorInfo.Error { get { throw new NotImplementedException(); } }
        string IDataErrorInfo.this[string columnName] => Verify(columnName);

        public virtual bool HasError => false;
        protected virtual string Verify(string columnName) => string.Empty;
        protected void HasErrorUpdated() => OnPropertyChanged(nameof(HasError));
    }
}
