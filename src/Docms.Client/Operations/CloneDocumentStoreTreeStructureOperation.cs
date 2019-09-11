using Docms.Client.Documents;
using Docms.Client.Types;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.Operations
{
    internal class CloneDocumentStoreTreeStructureOperation : IOperation
    {
        private readonly ApplicationContext context;

        public CloneDocumentStoreTreeStructureOperation(ApplicationContext context)
        {
            this.context = context;
        }

        public Task ExecuteAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var remoteDocuments = context.RemoteStorage.Root.ListAllDocuments();
            foreach (var doc in remoteDocuments)
            {
                token.ThrowIfCancellationRequested();
                var path = doc.Path;
                var parentDir = GetOrCreateDirectory(path.ParentPath);
                parentDir.AddChild(new DocumentNode(path.Name, doc.FileSize, doc.Hash, doc.Created, doc.LastModified));
            }
            return Task.CompletedTask;
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