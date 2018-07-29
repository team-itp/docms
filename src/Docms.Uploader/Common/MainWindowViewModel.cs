using Docms.Client;
using Docms.Uploader.FileWatch;
using Docms.Uploader.Properties;
using Docms.Uploader.Upload;
using System;
using System.Collections.Specialized;
using System.Linq;

namespace Docms.Uploader.Common
{
    public class MainWindowViewModel : ViewModelBase
    {
        public event EventHandler<EventArgs> SessionEnded;
        public event EventHandler<EventArgs> ShowSettingsRequested;

        private UploaderViewModel _uploader;
        private WatchingFileListViewModel _mediaFileList;
        private DocmsClient _client;

        public UploaderViewModel Uploader
        {
            get { return _uploader; }
            set
            {
                SetProperty(ref _uploader, value);
                OnPropertyChanged(nameof(Uploader));
            }
        }

        public WatchingFileListViewModel MediaFileList
        {
            get { return _mediaFileList; }
            set
            {
                SetProperty(ref _mediaFileList, value);
                OnPropertyChanged(nameof(MediaFileList));
            }
        }

        public RelayCommand LogoutCommand { get; }
        public RelayCommand ShowSettingsCommand { get; }

        // Design-Time only
        [Obsolete]
        public MainWindowViewModel() { }

        public MainWindowViewModel(DocmsClient client)
        {
            _client = client;
            LogoutCommand = new RelayCommand(Logout);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
        }

        public void ShowSettings()
        {
            ShowSettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        public async void Logout()
        {
            await _client.LogoutAsync();
            Settings.Default.UserId = "";
            Settings.Default.SetPasswordHash(null);
            Settings.Default.DirectoryToWatch = "";
            Settings.Default.Save();
            SessionEnded?.Invoke(this, EventArgs.Empty);
        }

        private void MediaFileListSelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems == null)
            {
                e.NewItems
                    .Cast<WatchingFileViewModel>()
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Add(vm));

            }
            else if (e.NewItems == null)
            {
                e.OldItems
                    .Cast<WatchingFileViewModel>()
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Remove(vm));
            }
            else
            {
                e.OldItems
                    .Cast<WatchingFileViewModel>()
                    .Except(e.NewItems.Cast<WatchingFileViewModel>())
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Remove(vm));
                e.NewItems
                    .Cast<WatchingFileViewModel>()
                    .Except(e.OldItems.Cast<WatchingFileViewModel>())
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Add(vm));
            }
        }

        public void Initialize()
        {
            var pathToWatch = Settings.Default.DirectoryToWatch;
            MediaFileList = new WatchingFileListViewModel(pathToWatch);
            Uploader = new UploaderViewModel(_client);

            MediaFileList.Startwatch();
            MediaFileList.SelectedFiles.CollectionChanged += MediaFileListSelectedFiles_CollectionChanged;
        }

        public void Terminate()
        {
            MediaFileList.Stopwatch();
        }
    }
}
