using Docms.Client.Documents;
using Docms.Client.Types;
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
    }
}