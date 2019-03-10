using System;
using System.Collections.Generic;
using System.Linq;

namespace Docms.Client.RemoteDocuments
{
    public class RemoteContainer : RemoteNode
    {
        private Dictionary<string, RemoteNode> children;

        public IEnumerable<RemoteNode> Children => children.Values.OrderBy(n => n.Name);

        private RemoteContainer() : base(null)
        {
            children = new Dictionary<string, RemoteNode>();
        }

        public RemoteContainer(string name) : base(name ?? throw new ArgumentNullException(nameof(name)))
        {
            children = new Dictionary<string, RemoteNode>();
        }

        public static RemoteContainer CreateRootContainer()
        {
            return new RemoteContainer();
        }

        public void AddChild(RemoteNode node)
        {
            if (children.ContainsKey(node.Name))
            {
                throw new InvalidOperationException();
            }
            children[node.Name] = node;
            node.SetParent(this);
        }

        public RemoteNode GetChild(string name)
        {
            if (children.TryGetValue(name, out var value))
            {
                return value;
            }
            return null;
        }
    }
}
