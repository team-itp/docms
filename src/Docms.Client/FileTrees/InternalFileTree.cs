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
        public DirectoryNode Root { get; } = new DirectoryNode("");

        public Node GetNode(PathString path)
        {
            if (path.Name == "")
            {
                return Root;
            }
            return GetDirectory(path.ParentPath)?.Get(path.Name);
        }

        public void Reset()
        {
            foreach (var item in Root.Children)
            {
                Root.Remove(item);
            }
        }

        public FileNode GetFile(PathString path)
        {
            return GetNode(path) as FileNode;
        }

        public DirectoryNode GetDirectory(PathString path)
        {
            return GetNode(path) as DirectoryNode;
        }

        private void EnsureDirectoryExists(PathString path)
        {
            var node = GetNode(path);
            if (node != null && !(node is DirectoryNode dir))
            {
                throw new InvalidOperationException();
            }
            if (node == null)
            {
                AddDirectory(path);
            }
        }

        public void AddFile(PathString path)
        {
            EnsureDirectoryExists(path.ParentPath);
            var dir = GetDirectory(path.ParentPath);
            dir.Add(new FileNode(path.Name));
        }

        public void AddDirectory(PathString path)
        {
            EnsureDirectoryExists(path.ParentPath);
            var dir = GetDirectory(path.ParentPath);
            dir.Add(new DirectoryNode(path.Name));
        }

        public void Move(PathString oldPath, PathString path)
        {
            var oldNode = GetNode(oldPath);
            var oldParent = oldNode.Parent;
            if (oldNode is DirectoryNode oldDir)
            {
                foreach (var childItem in oldDir.Children.ToArray())
                {
                    Move(oldPath.Combine(childItem.Name), path.Combine(childItem.Name));
                }
                if (oldNode.Parent != null)
                {
                    oldNode.Remove();
                }
                EnsureDirectoryExists(path);
            }
            else if (oldNode is FileNode oldFile)
            {
                EnsureDirectoryExists(path.ParentPath);
                var dir = GetDirectory(path.ParentPath);
                oldFile.Remove();
                oldFile.Name = path.Name;
                dir.Add(oldFile);

                if (!oldParent.Children.Any())
                {
                    oldParent.Remove();
                }
            }
        }

        public void Update(PathString path)
        {
            var node = GetNode(path);
            if (node == null)
            {
                throw new InvalidOperationException();
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
            }
        }

        public bool Exists(PathString path)
        {
            return GetNode(path) != null;
        }
    }
}
