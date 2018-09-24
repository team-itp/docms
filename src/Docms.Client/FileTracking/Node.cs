using System;
using System.Collections.Generic;

namespace Docms.Client.FileTracking
{
    public abstract class Node
    {
        public Node(string name)
        {
            Name = name;
        }

        public DirectoryNode Parent { get; set; }
        public string Name { get; set; }

        public void Remove()
        {
            if (Parent == null)
            {
                throw new InvalidOperationException();
            }
            Parent.Remove(this);
        }
    }

    public sealed class DirectoryNode : Node
    {
        private NodeCollection _children = new NodeCollection();

        public DirectoryNode(string name) : base(name)
        {
        }

        public IEnumerable<Node> Children => _children.Values;

        public Node Get(string name)
        {
            return _children.Get(name);
        }

        public DirectoryNode GetDirectory(string name)
        {
            return Get(name) as DirectoryNode;
        }

        public FileNode GetFile(string name)
        {
            return Get(name) as FileNode;
        }

        public void Add(Node node)
        {
            node.Parent = this;
            _children.Add(node);
        }

        public void Remove(Node node)
        {
            _children.Remove(node);
            node.Parent = null;
        }
    }

    public sealed class FileNode : Node
    {
        public Guid DataId { get; private set; }
        public string Hash { get; private set; }
        public long Size { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime LastModified { get; private set; }

        public FileNode(string name, Guid dataId, string hash, long size, DateTime created, DateTime lastModified) : base(name)
        {
            DataId = dataId;
            Hash = hash;
            Size = size;
            Created = created;
            LastModified = lastModified;
        }

        public void Rename(string name)
        {
            Name = name;
        }

        public void Update(string hash, long size, DateTime created, DateTime lastModified)
        {
            Hash = hash;
            Size = size;
            Created = created;
            LastModified = lastModified;
        }
    }

    public sealed class NodeCollection : Dictionary<string, Node>
    {
        public void Add(Node node)
        {
            Add(node.Name, node);
        }

        public void Remove(Node node)
        {
            Remove(node.Name);
        }

        public Node Get(string name)
        {
            if (TryGetValue(name, out var node))
            {
                return node;
            }
            return default(Node);
        }
    }
}
