using Docms.Client.FileStorage;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Docms.Client.FileTracking
{
    public class ShadowFileSystem
    {
        private FileTrackingEventShrinker EventShrinker = new FileTrackingEventShrinker();
        private IDataStorage _storage;

        public DirectoryNode Root { get; }

        public ShadowFileSystem(IDataStorage storage)
        {
            _storage = storage;
            Root = new DirectoryNode("");
        }

        public Node GetNode(PathString path)
        {
            if (path.Name == "")
            {
                return Root;
            }
            return GetDirectory(path.ParentPath)?.Get(path.Name);
        }

        public FileNode GetFile(PathString path)
        {
            return GetNode(path) as FileNode;
        }

        public DirectoryNode GetDirectory(PathString path)
        {
            return GetNode(path) as DirectoryNode;
        }

        public async Task CreateFileAsync(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            EnsureDirectoryExists(path.ParentPath);
            var dir = GetDirectory(path.ParentPath);
            var tempId = Guid.NewGuid();
            await _storage.SaveAsync(tempId, stream);
            var hash = Hash.CalculateHash(await _storage.OpenStreamAsync(tempId));
            var size = await _storage.GetSizeAsync(tempId);
            var file = new FileNode(path.Name, tempId, hash, size, created, lastModified);
            dir.Add(file);
            EventShrinker.Apply(new DocumentCreated(path));
        }

        private void EnsureDirectoryExists(PathString path)
        {
            if (path.ParentPath != null)
            {
                EnsureDirectoryExists(path.ParentPath);
                var node = GetNode(path);
                if (node is FileNode)
                {
                    throw new InvalidOperationException();
                }
                if (node == null)
                {
                    var dir = GetDirectory(path.ParentPath);
                    dir.Add(new DirectoryNode(path.Name));
                }
            }
        }

        public void CreateDirectory(PathString path)
        {
            EnsureDirectoryExists(path.ParentPath);
            var dir = GetDirectory(path.ParentPath);
            dir.Add(new DirectoryNode(path.Name));
        }

        public async Task MoveAsync(PathString oldPath, PathString path)
        {
            var overwritten = Exists(path);
            var oldNode = GetNode(oldPath);
            if (oldNode is DirectoryNode oldDir)
            {
                EnsureDirectoryExists(path);
                foreach (var childItem in oldDir.Children.ToArray())
                {
                    await MoveAsync(oldPath.Combine(childItem.Name), path.Combine(childItem.Name));
                }
                oldNode.Remove();
            }
            else if (oldNode is FileNode oldFile)
            {
                oldFile.Remove();
                oldFile.Rename(path.Name);
                EnsureDirectoryExists(path.ParentPath);
                var dir = GetDirectory(path.ParentPath);
                dir.Add(oldFile);
                EventShrinker.Apply(new DocumentMoved(path, oldPath));
            }
        }

        public async Task UpdateAsync(PathString path, Stream stream, DateTime created, DateTime lastModified)
        {
            var node = GetNode(path);
            if (node == null || !(node is FileNode file))
            {
                throw new InvalidOperationException();
            }

            var tempId = file.DataId;
            await _storage.SaveAsync(tempId, stream);
            var hash = Hash.CalculateHash(await _storage.OpenStreamAsync(tempId));
            var size = await _storage.GetSizeAsync(tempId);

            if (file.Hash == hash && file.Size == size)
            {
                return;
            }

            file.Update(hash, size, created, lastModified);
            EventShrinker.Apply(new DocumentUpdated(path));
        }

        public void Delete(PathString path)
        {
            if (!Exists(path))
            {
                throw new InvalidOperationException();
            }
            var node = GetNode(path);
            if (node is DirectoryNode dir)
            {
                foreach (var childItem in dir.Children.ToArray())
                {
                    Delete(path.Combine(childItem.Name));
                }
                node.Remove();
            }
            else if (node is FileNode file)
            {
                file.Remove();
                EventShrinker.Apply(new DocumentDeleted(path));
            }
        }

        public bool Exists(PathString path)
        {
            return GetNode(path) != null;
        }

        public IEnumerable<FileTrackingEvent> GetDelta()
        {
            return EventShrinker.Events;
        }

        public void ClearDelta()
        {
            EventShrinker.Reset();
        }
    }
}
