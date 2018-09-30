using Docms.Client.FileTracking;
using Docms.Client.SeedWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileStorage
{
    public class LocalFileStorageWatcher : ILocalFileStorageWatcher
    {
        public event EventHandler<FileCreatedEventArgs> FileCreated;
        public event EventHandler<FileModifiedEventArgs> FileModified;
        public event EventHandler<FileDeletedEventArgs> FileDeleted;
        public event EventHandler<FileMovedEventArgs> FileMoved;

        private readonly string _basePath;
        private ShadowFileSystem _shadowFileSystem;
        private FileSystemWatcher _watcher;
        private ConcurrentQueue<Func<Task>> _tasks = new ConcurrentQueue<Func<Task>>();

        public LocalFileStorageWatcher(string basePath, ShadowFileSystem shadowFileSystem)
        {
            _basePath = basePath;
            _shadowFileSystem = shadowFileSystem;
            EnsureDirectoryExists(_basePath);
            _watcher = new FileSystemWatcher(_basePath)
            {
                IncludeSubdirectories = true
            };
            _watcher.Created += new FileSystemEventHandler(_watcher_Created);
            _watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
            _watcher.Renamed += new RenamedEventHandler(_watcher_Renamed);
            _watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
            _watcher.Error += new ErrorEventHandler(_watcher_Error);
        }

        public Task StartWatch(CancellationToken cancellationToken = default(CancellationToken))
        {
            var tcs = new TaskCompletionSource<object>();
            _watcher.EnableRaisingEvents = true;
            _tasks.Enqueue(async () =>
            {
                var isOk = false;
                while (isOk)
                {
                    try
                    {
                        await StartTracking(PathString.Root, cancellationToken);
                        isOk = true;
                    }
                    catch
                    {

                    }
                }
            });

            return tcs.Task;
        }

        private async Task StartTracking(PathString path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileInfo = GetFile(path);
            if (fileInfo.Exists)
            {
                using (var fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    await _shadowFileSystem.CreateFileAsync(path, fs, fileInfo.CreationTimeUtc, fileInfo.LastWriteTimeUtc);
                }
            }
            var dirInfo = GetDirectory(path);
            if (dirInfo.Exists)
            {
                foreach (var item in GetFiles(path))
                {
                    await StartTracking(item, cancellationToken);
                }
                foreach (var item in GetDirectories(path))
                {
                    await StartTracking(item, cancellationToken);
                }
            }
        }

        public void StopWatch()
        {
            _watcher.EnableRaisingEvents = false;
        }

        private PathString ResolvePath(string fullPath)
        {
            return new PathString(fullPath.Substring(_basePath.Length + 1));
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath))
            {
                OnFileDeleted(ResolvePath(e.FullPath));
            }
            else if (!Directory.Exists(e.FullPath))
            {
                OnDirectoryDeleted(ResolvePath(e.FullPath));
            }
        }

        protected void OnFileDeleted(PathString path)
        {
            FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
        }

        protected void OnDirectoryDeleted(PathString path)
        {
            foreach (var item in GetFiles(path))
            {
                OnFileDeleted(item);
            }

            foreach (var item in GetDirectories(path))
            {
                OnDirectoryDeleted(item);
            }
        }

        private void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                OnFileMoved(ResolvePath(e.FullPath), ResolvePath(e.OldFullPath));
            }
            else if (Directory.Exists(e.FullPath))
            {
                OnDirectoryMoved(ResolvePath(e.FullPath), ResolvePath(e.OldFullPath));
            }
        }

        protected void OnFileMoved(PathString path, PathString fromPath)
        {
            FileMoved?.Invoke(this, new FileMovedEventArgs(path, fromPath));
        }

        protected void OnDirectoryMoved(PathString path, PathString fromPath)
        {
            foreach (var item in GetFiles(path))
            {
                OnFileMoved(path.Combine(item.Name), fromPath.Combine(item.Name));
            }

            foreach (var item in GetDirectories(path))
            {
                OnDirectoryMoved(path.Combine(item.Name), fromPath.Combine(item.Name));
            }
        }

        private void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                OnFileModified(ResolvePath(e.FullPath));
            }
            else if (Directory.Exists(e.FullPath))
            {
                OnDirectoryModified(ResolvePath(e.FullPath));
            }
        }

        protected void OnFileModified(PathString path)
        {
            FileModified?.Invoke(this, new FileModifiedEventArgs(path));
        }

        protected void OnDirectoryModified(PathString path)
        {
            foreach (var item in GetFiles(path))
            {
                OnFileModified(item);
            }

            foreach (var item in GetDirectories(path))
            {
                OnDirectoryModified(item);
            }
        }

        private void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                OnFileCreated(ResolvePath(e.FullPath));
            }
            else if (Directory.Exists(e.FullPath))
            {
                OnDirectoryCreated(ResolvePath(e.FullPath));
            }
        }

        protected void OnDirectoryCreated(PathString path)
        {
            foreach (var item in GetFiles(path))
            {
                OnFileCreated(item);
            }

            foreach (var item in GetDirectories(path))
            {
                OnDirectoryCreated(item);
            }
        }

        protected void OnFileCreated(PathString path)
        {
            FileCreated?.Invoke(this, new FileCreatedEventArgs(path));
        }

        public FileInfo GetFile(PathString path)
        {
            var fullpath = Path.Combine(_basePath, path.ToLocalPath());
            return new FileInfo(fullpath);
        }

        private DirectoryInfo GetDirectory(PathString path)
        {
            var fullpath = Path.Combine(_basePath, path.ToLocalPath());
            return new DirectoryInfo(fullpath);
        }

        public IEnumerable<PathString> GetFiles(PathString path)
        {
            return GetDirectory(path).GetFiles().Select(di => ResolvePath(di.FullName));
        }

        public IEnumerable<PathString> GetDirectories(PathString path)
        {
            return GetDirectory(path).GetDirectories().Select(di => ResolvePath(di.FullName));
        }

        private void EnsureDirectoryExists(string fullpath)
        {
            if (!Directory.Exists(fullpath))
            {
                Directory.CreateDirectory(fullpath);
            }
        }
    }
}
