using Docms.Client.SeedWork;
using System;

namespace Docms.Client.RemoteStorage
{
    public abstract class RemoteNode
    {
        protected RemoteNode()
        {
        }

        public RemoteNode(PathString path) : this()
        {
            Id = Guid.NewGuid();
            Path = path.ToString();
            ParentPath = path.ParentPath?.ToString();
            Name = path.Name;
        }

        public Guid Id { get; set; }
        public Guid ParentId { get; set; }
        public string Path { get; set; }
        public string ParentPath { get; set; }
        public string Name { get; set; }

        public virtual RemoteContainer Parent { get; set; }
    }
}
