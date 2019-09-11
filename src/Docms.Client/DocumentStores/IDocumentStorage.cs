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

        Task Initialize();
        ContainerNode GetContainer(PathString path);
        DocumentNode GetDocument(PathString path);
        Node GetNode(PathString path);
        Task SyncAsync(CancellationToken token = default);
    }
}