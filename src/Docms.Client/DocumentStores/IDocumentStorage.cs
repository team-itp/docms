using System.Collections.Generic;
using System.Threading.Tasks;
using Docms.Client.Data;
using Docms.Client.Documents;
using Docms.Client.Types;

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
    }
}