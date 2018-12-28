using Docms.Client.SeedWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Docms.Client.RemoteStorage
{
    public class RemoteContainer : RemoteNode
    {
        protected RemoteContainer() : base()
        {
            Children = new List<RemoteNode>();
        }

        public RemoteContainer(PathString path) : base(path)
        {
            Children = new List<RemoteNode>();
        }

        [InverseProperty("Parent")]
        public virtual ICollection<RemoteNode> Children { get; set; }

        public virtual void AddChild(RemoteNode child)
        {
            Children.Add(child);
            child.Parent = this;
            child.ParentId = this.Id;
        }
    }
}
