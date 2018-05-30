using System;
using System.ComponentModel;

namespace Docms.Uploader.Common
{
    public abstract class ViewModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        string IDataErrorInfo.Error { get { throw new NotImplementedException(); } }
        string IDataErrorInfo.this[string columnName] => Verify(columnName);

        public virtual bool HasError => false;
        protected virtual string Verify(string columnName) => string.Empty;
        protected void HasErrorUpdated() => OnPropertyChanged(nameof(HasError));
    }
}
