using Docms.Client.SeedWork;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Docms.Client.FileWatching
{
    public class InternalFileTree
    {
        public DirectoryNode Root { get; } = new DirectoryNode("");
        public ConcurrentQueue<LocalFileEventArgs> EventQueues = new ConcurrentQueue<LocalFileEventArgs>();

        public Node GetNode(PathString path)
        {
            if (path.Name == "")
            {
                return Root;
            }
            return GetDirectory(path.ParentPath)?.GetChild(path.Name);
        }

        public void Clear()
        {
            while (EventQueues.TryDequeue(out var ev))
            {
            }
            Root.Clear();
        }

        public FileNode GetFile(PathString path)
        {
            return GetNode(path) as FileNode;
        }

        public DirectoryNode GetDirectory(PathString path)
        {
            return GetNode(path) as DirectoryNode;
        }

        private DirectoryNode EnsureDirectoryExists(PathString path)
        {
            var node = GetNode(path);
            if (node == null)
            {
                var dir = EnsureDirectoryExists(path.ParentPath);
                var newDir = new DirectoryNode(path.Name);
                dir.AddChild(newDir);
                return newDir;
            }
            else if (node is DirectoryNode dir)
            {
                return dir;
            }
            throw new InvalidOperationException();
        }

        public bool AddFile(PathString path)
        {
            var dir = EnsureDirectoryExists(path.ParentPath);
            if (dir.AddChild(new FileNode(path.Name)))
            {
                EventQueues.Enqueue(new FileCreatedEventArgs(path));
                return true;
            }
            return false;
        }

        public bool Move(PathString oldPath, PathString path)
        {
            var destNode = GetNode(path);
            var srcNode = GetNode(oldPath);
            if (srcNode is FileNode srcFile)
            {
                if (destNode is DirectoryNode)
                {
                    throw new InvalidOperationException();
                }

                if (destNode != null && !Delete(path))
                {
                    return false;
                }

                var dir = EnsureDirectoryExists(path.ParentPath);
                if (srcFile.Rename(path.Name) && srcFile.MoveTo(dir))
                {
                    EventQueues.Enqueue(new FileMovedEventArgs(path, oldPath));
                    return true;
                }
            }
            else if (srcNode is DirectoryNode srcDir)
            {
                if (destNode is FileNode)
                {
                    throw new InvalidOperationException();
                }

                if (destNode != null && !Delete(path))
                {
                    return false;
                }
                var result = true;
                foreach (var node in srcDir.Children.ToArray())
                {
                    result &= Move(oldPath.Combine(node.Name), path.Combine(node.Name));
                }
                if (result)
                {
                    srcDir.Remove();
                    return true;
                }
            }
            return false;
        }

        public bool Update(PathString path)
        {
            var node = GetNode(path);
            if (node is DirectoryNode dir)
            {
                var result = true;
                foreach (var child in dir.Children)
                {
                    result &= Update(path.Combine(child.Name));
                }
                return result;
            }
            else if (node is FileNode file)
            {
                EventQueues.Enqueue(new FileModifiedEventArgs(path));
                return true;
            }
            return false;
        }

        public bool Delete(PathString path)
        {
            if (!Exists(path))
            {
                return false;
            }

            var node = GetNode(path);
            if (node is DirectoryNode dir)
            {
                var result = true;
                foreach (var childItem in dir.Children.ToArray())
                {
                    result &= Delete(path.Combine(childItem.Name));
                }
                if (result)
                {
                    node.Remove();
                    return true;
                }
            }
            else if (node is FileNode file)
            {
                if (file.Remove())
                {
                    EventQueues.Enqueue(new FileDeletedEventArgs(path));
                    return true;
                }
            }
            return false;
        }

        public bool Exists(PathString path)
        {
            return GetNode(path) != null;
        }
    }
}
