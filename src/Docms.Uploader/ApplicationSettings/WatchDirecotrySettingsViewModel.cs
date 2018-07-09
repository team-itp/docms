using Docms.Uploader.Properties;
using System.ComponentModel.DataAnnotations;

namespace Docms.Uploader.ApplicationSettings
{
    public class WatchDirecotrySettingsViewModel : SettingsViewModelBase
    {
        [Required]
        public string WatchDirectory
        {
            get => Settings.Default.DirectoryToWatch;
            set
            {
                Settings.Default.DirectoryToWatch = value;
                OnPropertyChanged(nameof(WatchDirectory));
            }
        }

        public WatchDirecotrySettingsViewModel()
        {
            OnPropertyChanged(nameof(WatchDirectory));
        }
    }
}