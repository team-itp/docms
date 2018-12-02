using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.FileWatching
{
    public abstract class LocalNode
    {
        public LocalNode(string name)
        {
            Name = name;
        }

        public LocalDirectoryNode Parent { get; set; }
        public string Name { get; set; }

        public bool Remove()
        {
            if (Parent == null)
            {
                return false;
            }
            return Parent.RemoveChild(this);
        }

        public bool MoveTo(LocalDirectoryNode destDir)
        {
            if (Parent == null)
            {
                return false;
            }
            if (Parent == destDir)
            {
                return true;
            }
            return Parent.RemoveChild(this) && destDir.AddChild(this);
        }

        public bool Rename(string newName)
        {
            if (Parent == null)
            {
                return false;
            }
            return Parent.RenameChild(this.Name, newName);
        }
    }

    public sealed class LocalDirectoryNode : LocalNode
    {
        private sealed class NodeCollection : Dictionary<string, LocalNode>
        {
            public bool Add(LocalNode node)
            {
                var contains = ContainsKey(node.Name);
                if (!contains)
                {
                    Add(node.Name, node);
                    return true;
                }
                return false;
            }

            public bool Remove(LocalNode node)
            {
                return Remove(node.Name);
            }

            public new void Clear()
            {
                foreach (var dir in this.OfType<LocalDirectoryNode>())
                {
                    dir.Clear();
                }
            }

            public LocalNode Get(string name)
            {
                if (TryGetValue(name, out var node))
                {
                    return node;
                }
                return default(LocalNode);
            }

            public bool Rename(string name, string newName)
            {
                if (TryGetValue(name, out var node))
                {
                    if (Remove(node.Name))
                    {
                        node.Name = newName;
                        return Add(node);
                    }
                }
                return false;
            }
        }

        private NodeCollection _children = new NodeCollection();

        public LocalDirectoryNode(string name) : base(name)
        {
        }

        public IEnumerable<LocalNode> Children => _children.Values;
        public bool IsEmpty => _children.Count == 0;

        public LocalNode GetChild(string name)
        {
            return _children.Get(name);
        }

        public LocalDirectoryNode GetDirectory(string name)
        {
            return GetChild(name) as LocalDirectoryNode;
        }

        public LocalFileNode GetFile(string name)
        {
            return GetChild(name) as LocalFileNode;
        }

        public bool AddChild(LocalNode node)
        {
            node.Parent = this;
            return _children.Add(node);
        }

        public bool RemoveChild(LocalNode node)
        {
            if (node.Parent != this)
            {
                throw new InvalidOperationException();
            }

            try
            {
                node.Parent = null;
                return _children.Remove(node);
            }
            finally
            {
                if (IsEmpty) Remove();
            }
        }

        public bool RenameChild(string name, string newName)
        {
            return _children.Rename(name, newName);
        }

        public void Clear()
        {
            _children.Clear();
        }
    }

    public sealed class LocalFileNode : LocalNode
    {
        public LocalFileNode(string name) : base(name)
        {
        }
    }
}
