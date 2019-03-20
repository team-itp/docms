using Docms.Client.Types;

namespace Docms.Client.Documents
{
    public abstract class Node
    {
        public ContainerNode Parent { get; private set; }

        public string Name { get; private set; }

        public PathString Path => Name == null
            ? PathString.Root
            : Parent?.Path.Combine(Name);

        public Node(string name)
        {
            Name = name;
        }

        internal void SetParent(ContainerNode parent)
        {
            Parent = parent;
        }

        internal void ClearParent()
        {
            Parent = null;
        }
    }
}