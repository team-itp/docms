using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.FileTrees
{
    public abstract class Node
    {
        public Node(string name)
        {
            Name = name;
        }

        public DirectoryNode Parent { get; set; }
        public string Name { get; set; }

        public bool Remove()
        {
            if (Parent == null)
            {
                return false;
            }
            return Parent.RemoveChild(this);
        }

        public bool MoveTo(DirectoryNode destDir)
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

    public sealed class DirectoryNode : Node
    {
        private sealed class NodeCollection : Dictionary<string, Node>
        {
            public bool Add(Node node)
            {
                var contains = ContainsKey(node.Name);
                if (!contains)
                {
                    Add(node.Name, node);
                    return true;
                }
                return false;
            }

            public bool Remove(Node node)
            {
                return Remove(node.Name);
            }

            public new void Clear()
            {
                foreach (var dir in this.OfType<DirectoryNode>())
                {
                    dir.Clear();
                }
            }

            public Node Get(string name)
            {
                if (TryGetValue(name, out var node))
                {
                    return node;
                }
                return default(Node);
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

        public DirectoryNode(string name) : base(name)
        {
        }

        public IEnumerable<Node> Children => _children.Values;
        public bool IsEmpty => _children.Count == 0;

        public Node GetChild(string name)
        {
            return _children.Get(name);
        }

        public DirectoryNode GetDirectory(string name)
        {
            return GetChild(name) as DirectoryNode;
        }

        public FileNode GetFile(string name)
        {
            return GetChild(name) as FileNode;
        }

        public bool AddChild(Node node)
        {
            node.Parent = this;
            return _children.Add(node);
        }

        public bool RemoveChild(Node node)
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

    public sealed class FileNode : Node
    {
        public FileNode(string name) : base(name)
        {
        }
    }
}
