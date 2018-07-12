using Docms.Uploader.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Uploader.ApplicationSettings
{
    public class SettingsViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs> SettingsConfirmed;
        public event EventHandler<EventArgs> SettingsCanceled;

        private object _viewModel;
        private List<SettingsViewModelBase> _viewModels;
        private bool _valid = true;

        public object ViewModel
        {
            get { return _viewModel; }
            set
            {
                SetProperty(ref _viewModel, value);
            }
        }

        public RelayCommand ConfirmCommand { get; }
        public RelayCommand CancelCommand { get; }

        public SettingsViewModel()
        {
            _viewModels = new List<SettingsViewModelBase>()
            {
                new WatchDirecotrySettingsViewModel(),
            };
            foreach (var vm in _viewModels)
            {
                vm.ErrorsChanged += Settings_ErrorsChanged;
            }
            ViewModel = _viewModels.First();

            ConfirmCommand = new RelayCommand(Confirm, () => _valid);
            CancelCommand = new RelayCommand(Cancel, () => true);
        }

        public void Confirm()
        {
            Properties.Settings.Default.Save();
            SettingsConfirmed?.Invoke(this, EventArgs.Empty);
        }

        public void Cancel()
        {
            Properties.Settings.Default.Reload();
            SettingsCanceled?.Invoke(this, EventArgs.Empty);
        }

        private void Settings_ErrorsChanged(object sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            _valid = !_viewModels.Any(vm => vm.HasErrors);
        }
    }

    public class SettingsViewModelBase : ValidationViewModelBase
    {
    }
}
