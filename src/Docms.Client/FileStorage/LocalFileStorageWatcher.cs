using Docms.Client.FileTrees;
using Docms.Client.SeedWork;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        private InternalFileTree _fileTree;
        private FileSystemWatcher _watcher;
        private ConcurrentQueue<Func<Task>> _tasks = new ConcurrentQueue<Func<Task>>();
        private CancellationTokenSource _processCts;
        private Task _processTask;
        private AutoResetEvent _taskHandle;

        public LocalFileStorageWatcher(string basePath)
        {
            _basePath = basePath;
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
            _taskHandle = new AutoResetEvent(false);
        }

        public async Task StartWatch(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_processTask != null)
                throw new InvalidOperationException();
            _processCts = new CancellationTokenSource();
            _processTask = ProcessAsync(_processCts.Token);
            _watcher.EnableRaisingEvents = true;
            _fileTree = new InternalFileTree();
            await EnqueueTask(async () =>
            {
                var isOk = false;
                while (isOk)
                {
                    try
                    {
                        await StartTracking(PathString.Root, cancellationToken);
                    }
                    catch
                    {
                    }
                }
            });
        }

        private async Task EnqueueTask(Func<Task> func)
        {
            var tcs = new TaskCompletionSource<object>();
            _tasks.Enqueue(async () =>
            {
                try
                {
                    await func();
                    tcs.SetResult(default(object));
                }
                catch (Exception ex)
                {
                    Trace.Write(ex);
                    tcs.SetException(ex);
                }
            });
            _taskHandle.Set();
            await tcs.Task;
        }

        private async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (_tasks.TryDequeue(out var func))
                    {
                        await func.Invoke();
                    }
                    else
                    {
                        WaitHandle.WaitAny(new WaitHandle[] { _taskHandle, cancellationToken.WaitHandle });
                        _taskHandle.Reset();
                    }
                }
            });
        }

        private async Task StartTracking(PathString path, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var fileInfo = GetFile(path);
            if (fileInfo.Exists)
            {
                using (var fs = fileInfo.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    _fileTree.AddFile(path);
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

        public async Task StopWatch()
        {
            if (_processTask != null)
            {
                _watcher.EnableRaisingEvents = false;
                _processCts.CancelAfter(100);
                await _processTask;
                _processTask = null;
            }
        }

        private PathString ResolvePath(string fullPath)
        {
            return new PathString(fullPath.Substring(_basePath.Length + 1));
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _watcher.EnableRaisingEvents = true;
        }

        private async void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    if (File.Exists(e.FullPath))
                    {
                        OnFileCreated(ResolvePath(e.FullPath));
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        OnDirectoryCreated(ResolvePath(e.FullPath));
                    }
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        private async void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    if (File.Exists(e.FullPath))
                    {
                        OnFileModified(ResolvePath(e.FullPath));
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        OnDirectoryModified(ResolvePath(e.FullPath));
                    }
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        private async void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    if (File.Exists(e.FullPath))
                    {
                        OnFileMoved(ResolvePath(e.FullPath), ResolvePath(e.OldFullPath));
                    }
                    else if (Directory.Exists(e.FullPath))
                    {
                        OnDirectoryMoved(ResolvePath(e.FullPath), ResolvePath(e.OldFullPath));
                    }
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        private async void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            try
            {
                await EnqueueTask(() =>
                {
                    if (!File.Exists(e.FullPath))
                    {
                        OnFileDeleted(ResolvePath(e.FullPath));
                    }
                    else if (!Directory.Exists(e.FullPath))
                    {
                        OnDirectoryDeleted(ResolvePath(e.FullPath));
                    }
                    return Task.CompletedTask;
                });
            }
            catch (Exception ex)
            {
                Trace.Write(ex);
            }
        }

        protected void OnFileCreated(PathString path)
        {
            _fileTree.AddFile(path);
            FileCreated?.Invoke(this, new FileCreatedEventArgs(path));
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

        protected void OnFileModified(PathString path)
        {
            _fileTree.Update(path);
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

        protected void OnFileMoved(PathString path, PathString fromPath)
        {
            _fileTree.Move(fromPath, path);
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

        protected void OnFileDeleted(PathString path)
        {
            _fileTree.Delete(path);
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
