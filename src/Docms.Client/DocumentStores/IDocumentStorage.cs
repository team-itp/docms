using Docms.Client.Documents;
using Docms.Client.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Docms.Client.DocumentStores
{
    public interface IDocumentStorage
    {
        ContainerNode Root { get; }

        ContainerNode GetContainer(PathString path);
        DocumentNode GetDocument(PathString path);
        Node GetNode(PathString path);
        Task Initialize();
        Task Save(CancellationToken token = default(CancellationToken));
        Task Save(DocumentNode documnet, CancellationToken token = default(CancellationToken));
        Task Sync(IProgress<int> progress = default(IProgress<int>), CancellationToken token = default(CancellationToken));
    }
}