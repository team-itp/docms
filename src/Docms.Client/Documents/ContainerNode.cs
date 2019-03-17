using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.Documents
{
    public class ContainerNode : Node
    {
        private Dictionary<string, Node> children;

        public IEnumerable<Node> Children => children.Values.OrderBy(n => n.Name);

        private ContainerNode() : base(null)
        {
            children = new Dictionary<string, Node>();
        }

        public ContainerNode(string name) : base(name ?? throw new ArgumentNullException(nameof(name)))
        {
            children = new Dictionary<string, Node>();
        }

        public static ContainerNode CreateRootContainer()
        {
            return new ContainerNode();
        }

        public void AddChild(Node node)
        {
            if (children.ContainsKey(node.Name))
            {
                throw new InvalidOperationException();
            }
            children[node.Name] = node;
            node.SetParent(this);
        }

        public Node GetChild(string name)
        {
            if (children.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }

        public void RemoveChild(Node remoteNode)
        {
            if (children.TryGetValue(remoteNode.Name, out var value) && remoteNode == value)
            {
                children.Remove(remoteNode.Name);
                remoteNode.ClearParent();
            }

            if (!children.Any() && Name != null && Parent != null)
            {
                Parent.RemoveChild(this);
            }
        }
    }
}
