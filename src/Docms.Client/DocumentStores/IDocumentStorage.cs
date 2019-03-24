using Docms.Client.Documents;
using Docms.Client.Types;
using System;
using System.IO;
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
        Task Save();
        Task Sync();

        Task<IDocumentStreamToken> ReadDocument(PathString path);
        Task WriteDocument(PathString path, Stream stream, DateTime created, DateTime lastModified);
    }
}