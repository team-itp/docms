using System;
using Docms.Client.Types;

namespace Docms.Client.RemoteDocuments
{
    public abstract class RemoteNode
    {
        public RemoteContainer Parent { get; private set; }

        public string Name { get; private set; }

        public PathString Path => Name == null ? PathString.Root : Parent.Path.Combine(Name);

        public RemoteNode(string name)
        {
            Name = name;
        }

        internal void SetParent(RemoteContainer parent)
        {
            Parent = parent;
        }
    }
}