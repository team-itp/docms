using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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
        private FileSystemWatcher _watcher;

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
        }

        public void StartWatch()
        {
            _watcher.EnableRaisingEvents = true;
        }

        public void StopWatch()
        {
            _watcher.EnableRaisingEvents = false;
        }

        private string ResolvePath(string fullPath)
        {
            return fullPath.Substring(_basePath.Length + 1);
        }

        private void _watcher_Error(object sender, ErrorEventArgs e)
        {
            _watcher.EnableRaisingEvents = true;
        }

        private void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if (File.Exists(e.FullPath))
            {
                OnFileDeleted(ResolvePath(e.FullPath));
            }
            else if (Directory.Exists(e.FullPath))
            {
                OnDirectoryDeleted(ResolvePath(e.FullPath));
            }
        }

        protected void OnFileDeleted(string path)
        {
            FileDeleted?.Invoke(this, new FileDeletedEventArgs(path));
        }

        protected void OnDirectoryDeleted(string path)
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
                OnFileModified(ResolvePath(e.FullPath));
            }
            else if (Directory.Exists(e.FullPath))
            {
                OnDirectoryModified(ResolvePath(e.FullPath));
            }
        }

        protected void OnFileMoved(string path, string fromPath)
        {
            FileMoved?.Invoke(this, new FileMovedEventArgs(path, fromPath));
        }

        protected void OnDirectoryMoved(string path, string fromPath)
        {
            foreach (var item in GetFiles(path))
            {
                var name = Path.GetFileName(item);
                OnFileMoved(Path.Combine(path, name), Path.Combine(fromPath, name));
            }

            foreach (var item in GetDirectories(path))
            {
                var name = Path.GetFileName(item);
                OnDirectoryMoved(Path.Combine(path, name), Path.Combine(fromPath, name));
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

        protected void OnFileModified(string path)
        {
            FileModified?.Invoke(this, new FileModifiedEventArgs(path));
        }

        protected void OnDirectoryModified(string path)
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

        protected void OnDirectoryCreated(string path)
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

        protected void OnFileCreated(string path)
        {
            FileCreated?.Invoke(this, new FileCreatedEventArgs(path));
        }

        public FileInfo GetFile(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return new FileInfo(fullpath);
        }

        public IEnumerable<string> GetFiles(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return Directory.GetFiles(fullpath).Select(ResolvePath);
        }

        public IEnumerable<string> GetDirectories(string path)
        {
            var fullpath = Path.Combine(_basePath, path);
            return Directory.GetDirectories(fullpath).Select(ResolvePath);
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
