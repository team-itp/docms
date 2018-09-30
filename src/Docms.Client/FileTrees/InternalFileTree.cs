using Docms.Client.FileStorage;
using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.FileTrees
{
    public class InternalFileTree
    {
        private FileTreeEventShrinker EventShrinker = new FileTreeEventShrinker();

        public DirectoryNode Root { get; } = new DirectoryNode("");

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

        public void AddFile(PathString path)
        {
            if (!Exists(path.ParentPath))
            {
                AddDirectory(path.ParentPath);
            }
            var dir = GetDirectory(path.ParentPath);
            dir.Add(new FileNode(path.Name));
            EventShrinker.Apply(new DocumentCreated(path));
        }

        public void AddDirectory(PathString path)
        {
            if (!Exists(path.ParentPath))
            {
                AddDirectory(path.ParentPath);
            }
            var dir = GetDirectory(path.ParentPath);
            dir.Add(new DirectoryNode(path.Name));
        }

        public void Move(PathString oldPath, PathString path)
        {
            var overwritten = Exists(path);
            var oldNode = GetNode(oldPath);
            if (oldNode is DirectoryNode oldDir)
            {
                foreach (var childItem in oldDir.Children.ToArray())
                {
                    Move(oldPath.Combine(childItem.Name), path.Combine(childItem.Name));
                }
            }
            else if (oldNode is FileNode oldFile)
            {
                oldFile.Remove();
                if (!Exists(path.ParentPath))
                {
                    AddDirectory(path.ParentPath);
                }
                var dir = GetDirectory(path.ParentPath);
                dir.Add(oldFile);
                EventShrinker.Apply(new DocumentMoved(path, oldPath));
            }
        }

        public void Update(PathString path)
        {
            var node = GetNode(path);
            if (node == null)
            {
                throw new InvalidOperationException();
            }
            if (node is FileNode file)
            {
                EventShrinker.Apply(new DocumentUpdated(path));
            }
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

        public IEnumerable<FileTreeEvent> GetDelta()
        {
            return EventShrinker.Events;
        }

        public void ClearDelta()
        {
            EventShrinker.Reset();
        }
    }
}
