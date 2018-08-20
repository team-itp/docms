using System.Collections.Generic;

namespace Docms.Client.FileTrees
{
    public abstract class Node
    {
        public DirectoryNode Parent { get; set; }
        public string Name { get; set; }
    }

    public sealed class DirectoryNode : Node
    {
        public NodeCollection Children { get; set; }
    }

    public sealed class FileNode : Node
    {
    }

    public sealed class NodeCollection : Dictionary<string, Node>
    {
    }
}
