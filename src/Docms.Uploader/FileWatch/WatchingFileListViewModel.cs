using Docms.Uploader.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Uploader.FileWatch
{

    public class WatchingFileListViewModel : ViewModelBase
    {
        public class WatchingFiles : ObservableCollection<WatchingFileViewModel>
        {
            private string _watchingRootPath;

            public WatchingFiles(string watchingRootPath)
            {
                _watchingRootPath = watchingRootPath;
            }

            public bool Add(string filepath)
            {
                foreach (var file in this.ToArray())
                {
                    if (file.FullPath == filepath)
                    {
                        return false;
                    }
                }
                this.Add(WatchingFileViewModel.Create(filepath, _watchingRootPath));
                return true;
            }

            public void Update(string oldFilePath, string newFilePath)
            {
                Remove(oldFilePath);
                Add(newFilePath);
            }

            public void Remove(string filepath)
            {
                foreach (var file in this.ToArray())
                {
                    if (file.FullPath == filepath)
                    {
                        this.Remove(file);
                        return;
                    }
                }
            }
        }

        public WatchingFiles Files { get; }
        public WatchingFiles SelectedFiles { get; }

        public bool IsWatching { get; private set; }
        public Task WatchTask { get; private set; }

        private SynchronizationContext _context;
        private string _pathToWatch;
        private FileSystemWatcher _watcher;
        private TaskCompletionSource<object> _tcs;

        public WatchingFileListViewModel(string pathToWatch)
        {
            _context = SynchronizationContext.Current;
            _pathToWatch = pathToWatch;
            _watcher = new FileSystemWatcher(_pathToWatch);
            _watcher.IncludeSubdirectories = true;
            _watcher.Created += new FileSystemEventHandler(_watcher_Created);
            _watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
            _watcher.Renamed += new RenamedEventHandler(_watcher_Renamed);
            _watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
            _watcher.Error += new ErrorEventHandler(_watcher_Error);

            Files = new WatchingFiles(_pathToWatch);
            SelectedFiles = new WatchingFiles(_pathToWatch);
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _tcs?.SetException(e.GetException());
            _tcs = null;
            IsWatching = false;
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            AddFileAsync(e.FullPath);
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            UpdateFileAsync(e.FullPath, e.FullPath);
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            UpdateFileAsync(e.OldFullPath, e.FullPath);
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            DeleteFileAsync(e.FullPath);
        }

        private bool CanLockFile(string filePath)
        {
            try
            {
                using (new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task AddFileAsync(string filePath)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(200);

                while (File.Exists(filePath))
                {
                    if (CanLockFile(filePath))
                    {
                        _context.Post(state => Files.Add(filePath), null);
                        return;
                    }

                    await Task.Delay(100);
                }
            });
        }

        public Task UpdateFileAsync(string oldFilePath, string newFilePath)
        {
            return Task.Run(async () =>
            {
                await Task.Delay(200);

                while (File.Exists(newFilePath))
                {
                    if (CanLockFile(newFilePath))
                    {
                        _context.Post(state => Files.Update(oldFilePath, newFilePath), null);
                        return;
                    }

                    await Task.Delay(100);
                }
            });
        }

        public Task DeleteFileAsync(string filePath)
        {
            _context.Send(state => Files.Remove(filePath), null);

            return Task.CompletedTask;
        }

        public Task Startwatch()
        {
            if (_watcher.EnableRaisingEvents)
            {
                return _tcs.Task;
            }

            _watcher.EnableRaisingEvents = true;
            IsWatching = true;

            var filesInDirectory = new List<string>();
            foreach (var filePath in Directory.GetFiles(_pathToWatch, "*", SearchOption.AllDirectories))
            {
                filesInDirectory.Add(filePath);
                Files.Add(filePath);
            }

            foreach (var filePath in Files.Select(mf => mf.FullPath).Except(filesInDirectory).ToArray())
            {
                Files.Remove(filePath);
            }

            _tcs = new TaskCompletionSource<object>();
            return _tcs.Task;
        }

        public void Stopwatch()
        {
            _watcher.EnableRaisingEvents = false;
            _tcs?.SetResult(null);
            _tcs = null;
            IsWatching = false;
        }
    }
}
