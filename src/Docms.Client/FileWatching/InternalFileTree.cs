using Docms.Client.SeedWork;
using System;
using System.Linq;

namespace Docms.Client.FileWatching
{
    public class InternalFileTree
    {
        public DirectoryNode Root { get; } = new DirectoryNode("");

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
            return dir.AddChild(new FileNode(path.Name));
        }

        public bool Move(PathString oldPath, PathString path)
        {
            var destNode = GetNode(path);
            var srcNode = GetNode(oldPath);
            if (destNode != null)
            {
                if (destNode is DirectoryNode destDir && srcNode is DirectoryNode srcDir)
                {
                    foreach (var node in srcDir.Children)
                    {
                        node.MoveTo(destDir);
                    }
                    srcDir.Remove();
                    return true;
                }
                else if (destNode is FileNode destFile && srcNode is FileNode srcFile)
                {
                    destFile.Remove();
                    var dir = EnsureDirectoryExists(path.ParentPath);
                    return srcFile.MoveTo(dir);
                }
                throw new InvalidOperationException();
            }
            else
            {
                var dir = EnsureDirectoryExists(path.ParentPath);
                return srcNode.Rename(path.Name) && srcNode.MoveTo(dir);
            }
        }

        public bool Update(PathString path)
        {
            var node = GetNode(path);
            return node != null;
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
            }
        }

        public bool Exists(PathString path)
        {
            return GetNode(path) != null;
        }
    }
}
