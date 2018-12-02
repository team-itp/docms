using Docms.Client.SeedWork;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Docms.Client.FileWatching
{
    public class InternalFileTree
    {
        public LocalDirectoryNode Root { get; } = new LocalDirectoryNode("");
        public ConcurrentQueue<LocalFileEventArgs> EventQueues = new ConcurrentQueue<LocalFileEventArgs>();

        public LocalNode GetNode(PathString path)
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

        public LocalFileNode GetFile(PathString path)
        {
            return GetNode(path) as LocalFileNode;
        }

        public LocalDirectoryNode GetDirectory(PathString path)
        {
            return GetNode(path) as LocalDirectoryNode;
        }

        private LocalDirectoryNode EnsureDirectoryExists(PathString path)
        {
            var node = GetNode(path);
            if (node == null)
            {
                var dir = EnsureDirectoryExists(path.ParentPath);
                var newDir = new LocalDirectoryNode(path.Name);
                dir.AddChild(newDir);
                return newDir;
            }
            else if (node is LocalDirectoryNode dir)
            {
                return dir;
            }
            throw new InvalidOperationException();
        }

        public bool AddFile(PathString path)
        {
            var dir = EnsureDirectoryExists(path.ParentPath);
            if (dir.AddChild(new LocalFileNode(path.Name)))
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
            if (srcNode is LocalFileNode srcFile)
            {
                if (destNode is LocalDirectoryNode)
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
            else if (srcNode is LocalDirectoryNode srcDir)
            {
                if (destNode is LocalFileNode)
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
            if (node is LocalDirectoryNode dir)
            {
                var result = true;
                foreach (var child in dir.Children)
                {
                    result &= Update(path.Combine(child.Name));
                }
                return result;
            }
            else if (node is LocalFileNode file)
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
            if (node is LocalDirectoryNode dir)
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
            else if (node is LocalFileNode file)
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
