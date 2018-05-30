﻿using Docms.Uploader.FileWatch;
using Docms.Uploader.Properties;
using Docms.Uploader.Upload;
using System.Collections.Specialized;
using System.Linq;

namespace Docms.Uploader.Common
{
    public class MainWindowViewModel : ViewModelBase
    {
        private UploaderViewModel _uploader;
        private MediaFileListViewModel _mediaFileList;

        public UploaderViewModel Uploader
        {
            get => _uploader;
            set
            {
                _uploader = value;
                OnPropertyChanged(nameof(Uploader));
            }
        }

        public MediaFileListViewModel MediaFileList
        {
            get => _mediaFileList;
            set
            {
                _mediaFileList = value;
                OnPropertyChanged(nameof(MediaFileList));
            }
        }

        public MainWindowViewModel()
        {
            var pathToWatch = Settings.Default.DirectoryToWatch;
            MediaFileList = new MediaFileListViewModel(pathToWatch);
            Uploader = new UploaderViewModel();

            MediaFileList.Startwatch();
            MediaFileList.SelectedFiles.CollectionChanged += MediaFileListSelectedFiles_CollectionChanged;
        }

        private void MediaFileListSelectedFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems == null)
            {
                e.NewItems
                    .Cast<MediaFile>()
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Add(vm));

            }
            else if (e.NewItems == null)
            {
                e.OldItems
                    .Cast<MediaFile>()
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Remove(vm));
            }
            else
            {
                e.OldItems
                    .Cast<MediaFile>()
                    .Except(e.NewItems.Cast<MediaFile>())
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Remove(vm));
                e.NewItems
                    .Cast<MediaFile>()
                    .Except(e.OldItems.Cast<MediaFile>())
                    .ToList()
                    .ForEach(vm => Uploader.SelectedMediaFiles.Add(vm));
            }
        }
    }
}
