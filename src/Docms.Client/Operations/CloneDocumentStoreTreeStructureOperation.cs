using Docms.Client.Documents;
using Docms.Client.Types;
using System.Threading;

namespace Docms.Client.Operations
{
    internal class CloneDocumentStoreTreeStructureOperation : OperationBase
    {
        private ApplicationContext context;

        public CloneDocumentStoreTreeStructureOperation(ApplicationContext context)
        {
            this.context = context;
        }

        protected override void Execute(CancellationToken token)
        {
            var remoteDocuments = context.RemoteStorage.Root.ListAllDocuments();
            foreach (var doc in remoteDocuments)
            {
                var path = doc.Path;
                var parentDir = GetOrCreateDirectory(path.ParentPath);
                parentDir.AddChild(new DocumentNode(path.Name, doc.FileSize, doc.Hash, doc.Created, doc.LastModified));
            }
        }

        private ContainerNode GetOrCreateDirectory(PathString path)
        {
            var dir = context.LocalStorage.GetContainer(path);
            if (dir == null)
            {
                var parentDir = GetOrCreateDirectory(path.ParentPath);
                dir = new ContainerNode(path.Name);
                parentDir.AddChild(dir);
            }
            return dir;
        }
    }
}